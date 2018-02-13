using CommandLine;

namespace EagleEye.FileImporter.CmdOptions
{
    [Verb("check", HelpText = "Remove dead files from index")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class CheckIndexOptions
    {
        [Option('o', "output-file", HelpText = "Filename containing all indexes", Required = true)]
        public string OutputFile { get; set; }
    }
}