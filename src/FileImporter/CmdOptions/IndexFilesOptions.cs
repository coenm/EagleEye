namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("index", HelpText = "Index files")]
    [UsedImplicitly]
    internal class IndexFilesOptions
    {
        [Option(shortName: 'd', longName: "directory", Required = true)]
        public string Directory { get; set; }
    }
}
