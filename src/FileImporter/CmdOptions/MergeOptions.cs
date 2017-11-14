using CommandLine;

namespace FileImporter.CmdOptions
{
    [Verb("merge", HelpText = "merge index files")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class MergeOptions
    {
        [Option('f', "first", HelpText = "First file", Required = true)]
        public string InputFile1 { get; set; }

        [Option('s', "second", HelpText = "Second file", Required = true)]
        public string InputFile2 { get; set; }
        
        [Option('o', "output-file", HelpText = "Filename with the results", Required = true)]
        public string OutputFile { get; set; }

        [Option("overwrite", HelpText = "Overwrite output file", Required = false, Default = false)]
        public bool OverwriteOutput { get; set; }
    }
}