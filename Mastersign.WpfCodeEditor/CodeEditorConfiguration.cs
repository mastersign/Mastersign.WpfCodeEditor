using System.Text.Json.Serialization;

namespace Mastersign.WpfCodeEditor;

public class CodeEditorConfiguration
{
    public bool EnableSchemaRequests { get; set; } = true;

    public bool ShowBreadcrumbs { get; set; } = true;

    public bool ShowCodeMarkers { get; set; } = true;

    /// <summary>
    /// The height of the code markers section as a CSS dimension.
    /// To use a percentage in height, use the CSS unit <code>vh</code>.
    /// To use an absolute height, use an absolute CSS unit like <code>px</code>.
    /// 
    /// The default value is <code>20vh</code>.
    /// </summary>
    public string CodeMarkersHeight { get; set; } = "20vh";

    public LineNumberStyle LineNumbers { get; set; } = LineNumberStyle.On;

    public ScrollbarVisibility VerticalScrollbar { get; set; } = ScrollbarVisibility.Auto;

    public ScrollbarVisibility HorizontalScrollbar { get; set; } = ScrollbarVisibility.Auto;

    public bool MinimapEnabled { get; set; } = true;

    public bool MinimapAutohide { get; set; } = false;

    public MinimapSide MinimapSide { get; set; } = MinimapSide.Right;

    public MinimapSliderVisibility MinimapShowSlider { get; set; } = MinimapSliderVisibility.MouseOver;

    public string LightTheme { get; set; } = "vs-light";

    public string DarkTheme { get; set; } = "vs-dark";
}

public enum LineNumberStyle
{
    On,
    Off,
    Relative,
    Interval,
}

public enum ScrollbarVisibility
{
    Auto,
    Visible,
    Hidden,
}

public enum MinimapSide
{
    Right,
    Left,
}

public enum MinimapSliderVisibility
{
    Always,

    [JsonStringEnumMemberName("mouseover")]
    MouseOver,
}
