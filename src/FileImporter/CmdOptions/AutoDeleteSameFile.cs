namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("autodelete", HelpText = "Remove files with specific file name convention that have a duplicate in same folder")]
    [UsedImplicitly]
    internal class AutoDeleteSameFile
    {
        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}
