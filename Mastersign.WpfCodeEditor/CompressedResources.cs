using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Mastersign.WpfCodeEditor
{
    internal class CompressedResources
    {
        private static readonly Dictionary<string, string> contentTypes = new()
        {
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".ttf", "font/ttf" },
        };
        
        private readonly ZipArchive archive;

        public CompressedResources(string archiveResourceName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(archiveResourceName);
            if (stream is null) throw new ArgumentException("Embedded resource not found: " + archiveResourceName);
            archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        }

        public virtual WebResource GetResource(string path)
        {
            ZipArchiveEntry entry;
            try
            {
                entry = archive.GetEntry(path.TrimStart('/'));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
            if (entry is null) return null;
            var ext = Path.GetExtension(path);
            var contentType = contentTypes.TryGetValue(ext, out var ct)
                ? ct
                : "application/octet-stream";
            return new WebResource(entry.Open(), entry.Length, contentType);
        }
    }
}
