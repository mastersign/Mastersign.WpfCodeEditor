namespace Mastersign.WpfCodeEditor;

public class CodeEditorConfiguration
{
    public bool EnableSchemaRequests { get; set; } = true;

    public bool ShowBreadcrumbs { get; set; } = true;

    public bool ShowCodeMarkers { get; set; } = true;

    public string LightTheme { get; set; } = "vs-light";

    public string DarkTheme { get; set; } = "vs-dark";
}