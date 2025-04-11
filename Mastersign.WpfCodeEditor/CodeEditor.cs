using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Mastersign.WpfCodeEditor
{
    public class CodeEditor : Control
    {
        private static string webviewDataDirectory = Path.Combine(
            Path.GetTempPath(),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
            + "_" + System.Reflection.Assembly.GetExecutingAssembly()
                .GetName().Version.ToString(2).Replace('.', '_'));

        public static string WebViewDataDirectory
        {
            get { return webviewDataDirectory; }
            set { webviewDataDirectory = value; }
        }

        static CodeEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeEditor), new FrameworkPropertyMetadata(typeof(CodeEditor)));
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

        private void CoreWebView2InitializationCompletedHandler(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            WebView.CoreWebView2.AddWebResourceRequestedFilter(
                "https://*.mastersign-code-editor/*",
                CoreWebView2WebResourceContext.All,
                CoreWebView2WebResourceRequestSourceKinds.All);

            WebView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            WebView.CoreWebView2.Navigate("https://monaco.mastersign-code-editor/index.html");
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (e.Request.Method != "GET")
            {
                e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                    Stream.Null, 405, "Method Not Allowed", string.Empty);
                return;
            }
            var url = new Uri(e.Request.Uri);
            var archiveName = url.Host.Replace(".mastersign-code-editor", "");
            var archive = GetEmbeddedResources(archiveName);
            if (archive is null)
            {
                Debug.WriteLine("Embedded compressed resources not found: " + archiveName);
                e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                    Stream.Null, 404, "Resource Not Found", string.Empty);
                return;
            }
            var resource = archive.GetResource(url.AbsolutePath);
            if (resource is null)
            {
                Debug.WriteLine("Compressed resource not found: " + archiveName + ":" + url.AbsolutePath);
                e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                    Stream.Null, 404, "Resource Not Found", string.Empty);
                return;
            }
            var headers = new StringBuilder();
            headers.Append("Content-Type: ");
            headers.Append(resource.ContentType);
            headers.AppendLine();
            headers.Append("Content-Length: ");
            headers.Append(resource.ContentLength);
            e.Response = WebView.CoreWebView2.Environment.CreateWebResourceResponse(
                resource.ResourceStream, 200, "OK", headers.ToString());
        }

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

        public void Navigate(string url)
        {
            if (WebView != null && WebView.CoreWebView2 != null)
            {
                WebView.CoreWebView2.Navigate(url);
            }
        }
    }
}