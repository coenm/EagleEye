namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("move", HelpText = "Move files to folders with date")]
    [UsedImplicitly]
    internal class MoveOptions
    {
        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }
    }
}
