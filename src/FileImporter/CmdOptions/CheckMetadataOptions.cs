namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("check", HelpText = "Check metadata.")]
    [UsedImplicitly]
    internal class CheckMetadataOptions
    {
        [Option(shortName: 'd', longName: "directory", HelpText = "Directory to find media files.", Required = true)]
        public string Directory { get; set; }
    }
}
