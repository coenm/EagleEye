namespace EagleEye.ExifToolWrapper.MediaInformationProviders.Parsing
{
    using System.Security.Cryptography;

    internal struct MetadataHeaderKeyPair
    {
        public MetadataHeaderKeyPair(string header1, string header2, string key)
        {
            Header1 = header1;
            Header2 = header2;
            Key = key;
        }

        public string Header1 { get; }

        public string Header2 { get; }

        public string Key { get; }

        public struct Keys
        {
            public const string XMP = "XMP";
            public const string COMPOSITE = "Composite";
            public const string EXIF = "EXIF";

            public const string EXIF_IFD = "ExifIFD";
            public const string XMP_EXIF = "XMP-exif";
        }
    }
}