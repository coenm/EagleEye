namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;

    [Verb("search")]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SearchOptions
    {
        [Option('d', "directory", HelpText = "Directory to process", Required = false)]
        public string DirectoryToIndex { get; set; }

        [Option('f', "File", Required = false)]
        public string IndexFiles2 { get; set; }

        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}