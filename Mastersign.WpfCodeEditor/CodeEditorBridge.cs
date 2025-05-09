using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Mastersign.WpfCodeEditor;

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class CodeEditorBridge
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly CodeEditor editor;

    internal CodeEditorBridge(CodeEditor editor)
    {
        this.editor = editor;
    }

    public void NotifyMonacoLoaded()
    {
        editor.Dispatcher.BeginInvoke(editor.MonacoLoadedHandler, null);
    }

    public void NotifyMonacoInitialized()
    {
        editor.Dispatcher.BeginInvoke(editor.MonacoInitializedHandler, null);
    }

    public void NotifyCurrentSymbols(string jsonSymbols)
    {
        var symbols = JsonSerializer.Deserialize<List<CodeSymbol>>(jsonSymbols, jsonOptions);
        //Debug.WriteLine(string.Join(" > ", symbols.Select(s => s.Name)));
        editor.Dispatcher.BeginInvoke(editor.CurrentSymbolsHandler, symbols);
    }

    public void NotifyMarkers(string jsonMarkers)
    {
        var markers = JsonSerializer.Deserialize<List<CodeMarker>>(jsonMarkers, jsonOptions);
        //Debug.WriteLine(JsonSerializer.Serialize(markers));
        editor.Dispatcher.BeginInvoke(editor.CodeMarkersHandler, markers);
    }
}
