namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;

    [Verb("update-similarity", HelpText = "Update similarities index files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UpdateSimilarityOptions
    {
        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}