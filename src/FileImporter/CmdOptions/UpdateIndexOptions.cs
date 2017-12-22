using CommandLine;

namespace FileImporter.CmdOptions
{
    [Verb("update", HelpText = "Update index files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class UpdateIndexOptions
    {
        [Option('d', "directory", HelpText = "Directory to process", Required = true)]
        public string DirectoryToIndex { get; set; }

        [Option('o', "output-file", HelpText = "Filename containing all indexes", Required = true)]
        public string OutputFile { get; set; }

        [Option('f', "force", Required = false, Default = false, HelpText = "ReIndex already existing files.")]
        public bool Force { get; set; }
    }
}