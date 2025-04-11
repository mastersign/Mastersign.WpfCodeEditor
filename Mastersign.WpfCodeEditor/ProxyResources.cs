using System;
using System.Diagnostics;
using System.Net.Http;

namespace Mastersign.WpfCodeEditor;

internal class ProxyResources
{
    private readonly HttpClient httpClient;

    public ProxyResources(string baseUrl)
    {
        httpClient = new HttpClient()
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(10),
        };
    }

    public WebResource GetResource(Uri url)
    {
        try
        {
            var response = httpClient.GetAsync(url.AbsolutePath).Result;
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var contentLength = response.Content.Headers.ContentLength;
            return new WebResource(
                response.Content.ReadAsStreamAsync().Result,
                contentLength, contentType);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return null;
        }
    }
}
