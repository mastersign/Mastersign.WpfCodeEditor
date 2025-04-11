using System.IO;

namespace Mastersign.WpfCodeEditor;

internal sealed class WebResource(Stream resourceStream, long? contentLength, string contentType)
{
    public Stream ResourceStream { get; } = resourceStream;

    public long? ContentLength { get; } = contentLength;

    public string ContentType { get; } = contentType;
}
