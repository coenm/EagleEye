namespace EagleEye.ExifTool.PhotoProvider.Parsing
{
    using Dawn;
    using JetBrains.Annotations;

    internal struct MetadataHeaderKeyPair
    {
        public MetadataHeaderKeyPair([NotNull] string header1, [NotNull] string header2, [NotNull] string key)
        {
            Guard.Argument(header1, nameof(header1)).NotNull();
            Guard.Argument(header2, nameof(header2)).NotNull();
            Guard.Argument(key, nameof(key)).NotNull();
            Header1 = header1;
            Header2 = header2;
            Key = key;
        }

        public string Header1 { get; }

        public string Header2 { get; }

        public string Key { get; }

        public struct Keys
        {
            public const string Xmp = "XMP";
            public const string Composite = "Composite";
            public const string Exif = "EXIF";

            public const string ExifIfd = "ExifIFD";
            public const string XmpExif = "XMP-exif";
        }
    }
}
