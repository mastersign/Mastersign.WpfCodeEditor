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
        editor.MonacoLoadedHandler();
    }

    public void NotifyCurrentSymbols(string jsonSymbols)
    {
        var symbols = JsonSerializer.Deserialize<List<CodeSymbol>>(jsonSymbols, jsonOptions);
        Debug.WriteLine(string.Join(" > ", symbols.Select(s => s.Name)));
    }

    public void NotifyMarkers(string jsonMarkers)
    {
        var markers = JsonSerializer.Deserialize<List<CodeMarker>>(jsonMarkers, jsonOptions);
        Debug.WriteLine(JsonSerializer.Serialize(markers));
    }
}
