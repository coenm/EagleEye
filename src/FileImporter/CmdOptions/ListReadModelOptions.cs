namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("list", HelpText = "list database)")]
    [UsedImplicitly]
    internal class ListReadModelOptions
    {
        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}
