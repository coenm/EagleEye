namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("update-similarity", HelpText = "Update similarities index files")]
    [UsedImplicitly]
    internal class UpdateSimilarityOptions
    {
        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}
