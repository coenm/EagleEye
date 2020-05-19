namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using EagleEye.Core.EagleEyeXmp;

    public class VerifyMediaResult
    {
        public VerifyMediaResult(string filename, MyState state, EagleEyeMetadata metadata)
        {
            Filename = filename;
            State = state;
            Metadata = metadata;
        }

        public string Filename { get; }

        public MyState State { get; }

        public EagleEyeMetadata Metadata { get; }

        public enum MyState
        {
            FileNotExist,

            NoMetadataAvailable,

            MetadataIncorrect,

            MetadataCorrect,
        }
    }
}