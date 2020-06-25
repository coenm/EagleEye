namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("update-picasa-ini", HelpText = "Update picasa ini files with person information when not available.")]
    [UsedImplicitly]
    internal class UpdatePicasaIniOptions
    {
        [Option(shortName: 'd', longName: "directory", HelpText = "Directory to find ini files.", Required = true)]
        public string Directory { get; set; }
    }
}
