namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("check", HelpText = "Remove dead files from index")]
    [UsedImplicitly]
    internal class CheckIndexOptions
    {
        [Option('o', "output-file", HelpText = "Filename containing all indexes", Required = true)]
        public string OutputFile { get; set; }
    }
}
