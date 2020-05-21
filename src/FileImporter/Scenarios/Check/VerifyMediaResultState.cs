namespace EagleEye.FileImporter.Scenarios.Check
{
    public enum VerifyMediaResultState
    {
        /// <summary>
        /// File does not exists.
        /// </summary>
        FileNotExist,

        /// <summary>
        /// File does not contain metadata.
        /// </summary>
        NoMetadataAvailable,

        /// <summary>
        /// Files metadata is incorrect.
        /// </summary>
        MetadataIncorrect,

        /// <summary>
        /// Files metadata is correct.
        /// </summary>
        MetadataCorrect,
    }
}
