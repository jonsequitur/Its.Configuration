using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Its.Configuration.Tests
{
    [Export]
    public class ImageRepository
    {
        [Import("cdn-api-uri", AllowDefault = true)]
        public Uri CdnApiUri { get; set; }

        [Import("image-location", AllowDefault = true)]
        public DirectoryInfo ImageLocation { get; set; }
    }
}