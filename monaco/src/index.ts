import { editor, languages, MarkerSeverity, type Position, Range, Uri  } from 'monaco-editor'
import * as monaco from 'monaco-editor'
import { ILanguageFeaturesService } from 'monaco-editor/esm/vs/editor/common/services/languageFeatures.js'
import { OutlineModel } from 'monaco-editor/esm/vs/editor/contrib/documentSymbols/browser/outlineModel.js'
import { StandaloneServices } from 'monaco-editor/esm/vs/editor/standalone/browser/standaloneServices.js'
import { configureMonacoYaml } from 'monaco-yaml'

import './index.css'

const wpfControl: any = chrome?.webview?.hostObjects?.wpfControl

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

const monacoYaml = configureMonacoYaml(monaco, {
  enableSchemaRequest: false,
  schemas: []
})

function loadSchema(schema: any, uri: string) {
  monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
    validate: true,
    schemas: [
      {
        uri,
        fileMatch: ['*.json'],
        schema,
      },
    ],
  })
  monacoYaml.update({
    enableSchemaRequest: false,
    schemas: [
      {
        uri,
        fileMatch: ['*.yaml', '*.yml'],
        schema,
      },
    ],
  })
}

const darkModePreference = window.matchMedia('(prefers-color-scheme: dark)')
const ed = editor.create(document.getElementById('editor')!, {
  automaticLayout: true,
  theme: darkModePreference.matches ? 'vs-dark' : 'vs-light',
  quickSuggestions: {
    other: true,
    comments: false,
    strings: true
  },
  formatOnType: true
})
darkModePreference.addEventListener('change', e => {
  ed.updateOptions({ theme: e.matches ? 'vs-dark' : 'vs-light'})
})

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

ed.onDidChangeCursorPosition(async (event) => {
  const breadcrumbs = document.getElementById('breadcrumbs')!
  const { documentSymbolProvider } = StandaloneServices.get(ILanguageFeaturesService)
  const outline = await OutlineModel.create(documentSymbolProvider, ed.getModel()!)
  const symbols = outline.asListOfDocumentSymbols()
  while (breadcrumbs.lastChild) {
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
    breadcrumbs.append(breadcrumb)
  }
  if (wpfControl) {
    wpfControl.NotifyCurrentSymbols(JSON.stringify(symbolList))
  }
})

editor.onDidChangeMarkers(([resource]) => {
  const problems = document.getElementById('problems')!
  const markers = editor.getModelMarkers({ resource })
  while (problems.lastChild) {
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
      ed.setPosition({ lineNumber: marker.startLineNumber, column: marker.startColumn })
      ed.focus()
    })
    problems.append(wrapper)
  }
  if (wpfControl) {
    wpfControl.NotifyMarkers(JSON.stringify(markerList))
  }
})

function loadModel(content: string, language: string, filePath: string) {
  const oldModel = ed.getModel()
  const newModel = editor.createModel(content, language, Uri.parse(filePath))
  ed.setModel(newModel)
  oldModel?.dispose()
  ed.focus()
}

function getContent() {
  return ed.getModel()?.getValue()
}

function setCursorPosition(lineNumber: number, column: number) {
  ed.setPosition({ lineNumber, column })
}

function focus() {
  ed.focus()
}

window.mastersignCodeEditor = {
  loadSchema,
  loadModel,
  getContent,
  setCursorPosition,
  focus,
}

if (wpfControl) {
  wpfControl.NotifyMonacoLoaded()
}
