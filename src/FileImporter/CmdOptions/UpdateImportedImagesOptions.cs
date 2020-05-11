namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("update-imported-images", HelpText = "Add EagleEye metadata to image if not present.")]
    [UsedImplicitly]
    internal class UpdateImportedImagesOptions
    {
        [Option(shortName: 'd', longName: "processing-directory", HelpText = "File to find duplicates", Required = true)]
        public string ProcessingDirectory { get; set; }
    }
}
