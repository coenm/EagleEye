using CommandLine;

namespace FileImporter.CmdOptions
{
    [Verb("index", HelpText = "Index files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class IndexOptions
    {
        [Option('d', "directory", HelpText = "Directory to process", Required = true)]
        public string DirectoryToIndex { get; set; }

        [Option('o', "output-file", HelpText = "Filename with the results", Required = true)]
        public string OutputFile { get; set; }

        [Option("append-results", HelpText = "Append new files to original output file. Otherwise it will replace output file.", Required = false, Default = false)]
        public bool AppendResults { get; set; }
    }
}