namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;

    [Verb("move", HelpText = "Move files to folders with date")]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MoveOptions
    {
        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }
    }
}