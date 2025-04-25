import { editor, languages, MarkerSeverity, type Position, Range, Uri  } from 'monaco-editor'
import * as monaco from 'monaco-editor'
import { ILanguageFeaturesService } from 'monaco-editor/esm/vs/editor/common/services/languageFeatures.js'
import { OutlineModel } from 'monaco-editor/esm/vs/editor/contrib/documentSymbols/browser/outlineModel.js'
import { StandaloneServices } from 'monaco-editor/esm/vs/editor/standalone/browser/standaloneServices.js'
import { configureMonacoYaml, MonacoYaml } from 'monaco-yaml'

import './index.css'

window.MonacoEnvironment = {
  getWorker(moduleId, label) {
    switch (label) {
      case 'editorWorkerService':
        return new Worker(new URL('monaco-editor/esm/vs/editor/editor.worker', import.meta.url))
      // case 'css':
      // case 'less':
      // case 'scss':
      //   return new Worker(new URL('monaco-editor/esm/vs/language/css/css.worker', import.meta.url))
      // case 'handlebars':
      // case 'html':
      // case 'razor':
      //   return new Worker(
      //     new URL('monaco-editor/esm/vs/language/html/html.worker', import.meta.url)
      //   )
      case 'json':
        return new Worker(new URL('monaco-editor/esm/vs/language/json/json.worker', import.meta.url)
        )
      // case 'javascript':
      // case 'typescript':
      //   return new Worker(
      //     new URL('monaco-editor/esm/vs/language/typescript/ts.worker', import.meta.url)
      //   )
      case 'yaml':
        return new Worker(new URL('monaco-yaml/yaml.worker', import.meta.url))
      default:
        throw new Error(`Unknown label ${label}`)
    }
  }
}

const wpfControl: any = chrome?.webview?.hostObjects?.wpfControl

interface Configuration {
  enableSchemaRequests: boolean,
  showBreadcrumbs: boolean,
  showCodeMarkers: boolean,
  lightTheme: string,
  darkTheme: string,
}

interface State {
  monacoYaml: MonacoYaml | null,
  editor: editor.IStandaloneCodeEditor | null,
}

let configuration: Configuration = {
  enableSchemaRequests: true,
  showBreadcrumbs: true,
  showCodeMarkers: true,
  lightTheme: 'vs-light',
  darkTheme: 'vs-dark',
}

const state: State = {
  monacoYaml: null,
  editor: null,
}

function initialize(config: Configuration) {
  configuration = {...configuration, ...config}

  state.monacoYaml?.dispose()

  state.monacoYaml = configureMonacoYaml(monaco, {
    enableSchemaRequest: configuration.enableSchemaRequests,
    schemas: []
  })

  state.editor?.dispose()

  // adjust layout
  const breadcrumbs = document.getElementById('breadcrumbs')!
  const problems = document.getElementById('problems')!
  breadcrumbs.style.display = configuration.showBreadcrumbs ? 'block' : 'none'
  problems.style.display = configuration.showCodeMarkers ? 'block' : 'none'

  state.editor = buildEditor()

  wpfControl?.NotifyMonacoInitialized()
}

function loadSchema(schema: any, uri: string) {
  if (!state.monacoYaml) return
  monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
    enableSchemaRequest: configuration.enableSchemaRequests,
    validate: true,
    schemas: [
      {
        uri,
        fileMatch: ['*.json'],
        schema,
      },
    ],
  })
  state.monacoYaml.update({
    enableSchemaRequest: configuration.enableSchemaRequests,
    schemas: [
      {
        uri,
        fileMatch: ['*.yaml', '*.yml'],
        schema,
      },
    ],
  })
}

/**
 * Get the document symbols that contain the given position.
 *
 * @param symbols
 *   The symbols to iterate.
 * @param position
 *   The position for which to filter document symbols.
 * @yields
 * The document symbols that contain the given position.
 */
function* iterateSymbols(
  symbols: languages.DocumentSymbol[],
  position: Position
): Iterable<languages.DocumentSymbol> {
  for (const symbol of symbols) {
    if (Range.containsPosition(symbol.range, position)) {
      yield symbol
      if (symbol.children) {
        yield* iterateSymbols(symbol.children, position)
      }
    }
  }
}

function buildEditor() {
  const darkModePreference = window.matchMedia('(prefers-color-scheme: dark)')
  const ed = editor.create(document.getElementById('editor')!, {
    automaticLayout: true,
    theme: darkModePreference.matches ? configuration.darkTheme : configuration.lightTheme,
    quickSuggestions: {
      other: true,
      comments: false,
      strings: true
    },
    formatOnType: true
  })
  darkModePreference.addEventListener('change', e => {
    ed.updateOptions({
      theme: e.matches ? configuration.darkTheme : configuration.lightTheme,
    })
  })

  ed.onDidChangeCursorPosition(async (event) => {
    const { documentSymbolProvider } = StandaloneServices.get(ILanguageFeaturesService)
    const outline = await OutlineModel.create(documentSymbolProvider, ed.getModel()!)
    const symbols = outline.asListOfDocumentSymbols()
    const breadcrumbs = document.getElementById('breadcrumbs')!
    while (breadcrumbs?.lastChild) {
      breadcrumbs.lastChild.remove()
    }
    const symbolList = []
    for (const symbol of iterateSymbols(symbols, event.position)) {
      symbolList.push({
        name: symbol.name,
        detail: symbol.detail,
        startLine: symbol.range.startLineNumber,
        startColumn: symbol.range.startColumn,
      })
      if (configuration.showBreadcrumbs) {
        const breadcrumb = document.createElement('span')
        breadcrumb.setAttribute('role', 'button')
        breadcrumb.classList.add('breadcrumb')
        breadcrumb.textContent = symbol.name
        breadcrumb.title = symbol.detail
        if (symbol.kind === languages.SymbolKind.Array) {
          breadcrumb.classList.add('array')
        } else if (symbol.kind === languages.SymbolKind.Module) {
          breadcrumb.classList.add('object')
        }
        breadcrumb.addEventListener('click', () => {
          ed.setPosition({
            lineNumber: symbol.range.startLineNumber,
            column: symbol.range.startColumn
          })
          ed.focus()
        })
        breadcrumbs?.append(breadcrumb)
      }
    }
    wpfControl?.NotifyCurrentSymbols(JSON.stringify(symbolList))
  })

  return ed
}

editor.onDidChangeMarkers(([resource]) => {
  const markers = editor.getModelMarkers({ resource })
  const problems = document.getElementById('problems')!
  while (problems?.lastChild) {
    problems.lastChild.remove()
  }
  const markerList = []
  for (const marker of markers) {
    markerList.push({
      startLineNumber: marker.startLineNumber,
      startColumn: marker.startColumn,
      endLineNumber: marker.endLineNumber,
      endColumn: marker.endColumn,
      message: marker.message,
      severity: marker.severity,
    })
    if (marker.severity === MarkerSeverity.Hint) {
      continue
    }
    if (configuration.showCodeMarkers) {
      const wrapper = document.createElement('div')
      wrapper.setAttribute('role', 'button')
      const codicon = document.createElement('div')
      const text = document.createElement('div')
      wrapper.classList.add('problem')
      codicon.classList.add(
        'codicon',
        marker.severity === MarkerSeverity.Warning ? 'codicon-warning' : 'codicon-error'
      )
      text.classList.add('problem-text')
      text.textContent = marker.message
      wrapper.append(codicon, text)
      wrapper.addEventListener('click', () => {
        state.editor?.setPosition({ lineNumber: marker.startLineNumber, column: marker.startColumn })
        state.editor?.focus()
      })
      problems?.append(wrapper)
    }
  }
  wpfControl?.NotifyMarkers(JSON.stringify(markerList))
})

function loadModel(content: string, language: string, filePath: string) {
  const ed = state.editor
  if (!ed) return
  const oldModel = ed.getModel()
  ed.setModel(null)
  oldModel?.dispose()
  try {
    const newModel = editor.createModel(content, language, Uri.parse(filePath))
    ed.setModel(newModel)
  } catch (err) {
  }
  ed.focus()
}

function getContent() {
  return state.editor?.getModel()?.getValue()
}

function setCursorPosition(lineNumber: number, column: number) {
  state.editor?.setPosition({ lineNumber, column })
}

function focus() {
  state.editor?.focus()
}

window.mastersignCodeEditor = {
  initialize,
  loadSchema,
  loadModel,
  getContent,
  setCursorPosition,
  focus,
}

wpfControl?.NotifyMonacoLoaded()
