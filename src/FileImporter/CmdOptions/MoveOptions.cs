using CommandLine;

namespace EagleEye.FileImporter.CmdOptions
{
    [Verb("move", HelpText = "Move files to folders with date")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class MoveOptions
    {
        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }
    }
}