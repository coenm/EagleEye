using CommandLine;

namespace EagleEye.FileImporter.CmdOptions
{
    [Verb("autodelete", HelpText = "Remove files with specific file name convention that have a duplicate in same folder")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class AutoDeleteSameFile
    {
        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}