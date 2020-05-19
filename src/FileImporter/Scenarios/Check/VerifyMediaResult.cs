namespace EagleEye.FileImporter.Scenarios.Check
{
    using EagleEye.Core.EagleEyeXmp;

    public class VerifyMediaResult
    {
        public VerifyMediaResult(string filename, VerifyMediaResultState state, EagleEyeMetadata metadata)
        {
            Filename = filename;
            State = state;
            Metadata = metadata;
        }

        public string Filename { get; }

        public VerifyMediaResultState State { get; }

        public EagleEyeMetadata Metadata { get; }
    }
}
