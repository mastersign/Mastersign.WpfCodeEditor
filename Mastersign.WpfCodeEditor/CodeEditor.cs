using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Mastersign.WpfCodeEditor;

public class CodeEditor : Control
{
    private const string EMBEDDED_RESOURCE_TLD = ".mastersign-code-editor";
    private const string MONACO_EDITOR_URL = $"https://monaco{EMBEDDED_RESOURCE_TLD}/index.html";

    private static string webviewDataDirectory = Path.Combine(
        Path.GetTempPath(),
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
        + "_" + System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version.ToString(2).Replace('.', '_'));

    private static readonly ProxyResources proxyResources = new("http://127.0.0.1:8080");

    public static string WebViewDataDirectory
    {
        get { return webviewDataDirectory; }
        set { webviewDataDirectory = value; }
    }

    private static readonly JsonSerializerOptions configurationSerializationOptions = new()
    {
        IndentSize = 2,
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    static CodeEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeEditor), new FrameworkPropertyMetadata(typeof(CodeEditor)));

        configurationSerializationOptions.Converters.Add(
            new JsonStringEnumConverter<LineNumberStyle>(JsonNamingPolicy.CamelCase));
        configurationSerializationOptions.Converters.Add(
            new JsonStringEnumConverter<ScrollbarVisibility>(JsonNamingPolicy.CamelCase));
        configurationSerializationOptions.Converters.Add(
            new JsonStringEnumConverter<MinimapSide>(JsonNamingPolicy.CamelCase));
        configurationSerializationOptions.Converters.Add(
            new JsonStringEnumConverter<MinimapSliderVisibility>(JsonNamingPolicy.CamelCase));
    }

    private static readonly Dictionary<string, CompressedResources> embeddedResources = [];

    private static CompressedResources GetEmbeddedResources(string archiveName)
    {
        var ns = typeof(CodeEditor).Namespace;
        lock (embeddedResources)
        {
            if (embeddedResources.TryGetValue(archiveName, out CompressedResources result))
            {
                return result;
            }
            else
            {
                try
                {
                    var cr = new CompressedResources($"{ns}.{archiveName}.zip");
                    embeddedResources.Add(archiveName, cr);
                    return cr;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    embeddedResources.Add(archiveName, null);
                    return null;
                }
            }
        }
    }

    private static WebResource GetEmbeddedResource(Uri url)
    {
        var archiveName = url.Host.Replace(EMBEDDED_RESOURCE_TLD, "");
        var archive = GetEmbeddedResources(archiveName);
        if (archive is null)
        {
            Debug.WriteLine("Embedded compressed resources not found: " + archiveName);
            return null;
        }
        var resource = archive.GetResource(url.AbsolutePath);
        if (resource is null)
        {
            Debug.WriteLine("Compressed resource not found: " + archiveName + ":" + url.AbsolutePath);
            return null;
        }
        return resource;
    }

    public bool UseDevelopmentProxy { get; set; }

    private WebView2 webview;

    private WebView2 WebView
    {
        get => webview;
        set
        {
            if (webview != null)
            {
                webview.CoreWebView2InitializationCompleted -= CoreWebView2InitializationCompletedHandler;
            }
            webview = value;
            if (webview != null)
            {
                webview.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompletedHandler;
            }
        }
    }

    public override void OnApplyTemplate()
    {
        WebView = GetTemplateChild("WebView") as WebView2;
        InitWebView();
    }

    private async void InitWebView()
    {
        var options = new CoreWebView2EnvironmentOptions(
            additionalBrowserArguments: null,
            language: null,
            targetCompatibleBrowserVersion: null,
            allowSingleSignOnUsingOSPrimaryAccount: false,
            customSchemeRegistrations: null,
            channelSearchKind: CoreWebView2ChannelSearchKind.MostStable);
        var env = await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: webviewDataDirectory,
            options: options);
        await WebView.EnsureCoreWebView2Async(env);
    }

    public CodeEditorConfiguration Configuration { get; set; } = new();

    private void CoreWebView2InitializationCompletedHandler(object sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
        WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
        WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        WebView.CoreWebView2.Settings.IsPinchZoomEnabled = false;
        WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;
#if DEBUG
        WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
#else
        WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif

        WebView.CoreWebView2.AddWebResourceRequestedFilter(
            $"https://*{EMBEDDED_RESOURCE_TLD}/*",
            CoreWebView2WebResourceContext.All,
            CoreWebView2WebResourceRequestSourceKinds.All);

        WebView.CoreWebView2.WebResourceRequested += WebResourceRequestHandler;

        WebView.CoreWebView2.AddHostObjectToScript("wpfControl", new CodeEditorBridge(this));

        WebView.CoreWebView2.Navigate(MONACO_EDITOR_URL);
    }

    private void WebResourceRequestHandler(object sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
        if (e.Request.Method != "GET")
        {
            e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                Stream.Null, 405, "Method Not Allowed", string.Empty);
            return;
        }
        var url = new Uri(e.Request.Uri);

        var resource = UseDevelopmentProxy
            ? proxyResources.GetResource(url)
            : GetEmbeddedResource(url);

        if (resource is null)
        {
            e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                Stream.Null, 404, "Resource Not Found", string.Empty);
            return;
        }

        var headers = new StringBuilder();
        headers.Append("Content-Type: ");
        headers.Append(resource.ContentType);
        if (resource.ContentLength.HasValue)
        {
            headers.AppendLine();
            headers.Append("Content-Length: ");
            headers.Append(resource.ContentLength.Value);
        }
        e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
            resource.ResourceStream, 200, "OK", headers.ToString());
    }

    internal async void MonacoLoadedHandler()
    {
        var configJson = JsonSerializer.Serialize(Configuration, configurationSerializationOptions);
        var jsCode = $"mastersignCodeEditor.initialize({configJson})";
        await WebView.CoreWebView2.ExecuteScriptAsync(jsCode);
    }

    public event EventHandler<EventArgs> EditorReady;

    internal void MonacoInitializedHandler()
    {
        EditorReady?.Invoke(this, EventArgs.Empty);
    }

    private readonly ObservableCollection<CodeSymbol> currentSymbols = [];

    public event EventHandler<EventArgs> CurrentSymbolsChanged;

    internal void CurrentSymbolsHandler(ICollection<CodeSymbol> symbols)
    {
        if (currentSymbols.UpdateSorted(symbols))
        {
            CurrentSymbolsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private readonly ObservableCollection<CodeMarker> markers = [];

    public event EventHandler<EventArgs> CodeMarkersChanged;

    internal void CodeMarkersHandler(ICollection<CodeMarker> markers)
    {
        if (this.markers.UpdateSorted(markers))
        {
            CodeMarkersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Reinitialize() => WebView?.CoreWebView2?.Reload();

    internal void Navigate(string url) => WebView?.CoreWebView2?.Navigate(url);

    public async Task LoadJsonSchema(string schema, string uri)
    {
        var jsCode = $"mastersignCodeEditor.loadSchema({schema}, '{uri}'); console.log('loaded schema');";
        await WebView.ExecuteScriptAsync(jsCode);
    }

    public async Task LoadText(string text, CodeLanguage language, string filename)
    {
        var languageName = Enum.GetName(typeof(CodeLanguage), language).ToLowerInvariant();
        var jsonText = JsonSerializer.Serialize(text);
        var jsCode = $"mastersignCodeEditor.loadModel({jsonText}, '{languageName}', '{filename}'); console.log('loaded text');";
        await WebView.ExecuteScriptAsync(jsCode);
    }

    public async Task<string> GetText()
    {
        var jsonResult = await WebView.ExecuteScriptAsync("mastersignCodeEditor.getContent()");
        var text = JsonSerializer.Deserialize<string>(jsonResult);
        return text.Replace(@"\n", Environment.NewLine);
    }
}
