namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;

    [Verb("search-file")]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SearchDuplicateFileOptions
    {
        [Option('f', "File", HelpText = "Original image", Required = true)]
        public string OriginalImageFile { get; set; }

        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }

        [Option('p', "prefix", HelpText = "Path resulting images", Required = false)]
        public string PathPrefix { get; set; }

        [Option('v', "value", HelpText = "Path resulting images", Required = false, Default = 95)]
        public double Value { get; set; }
    }
}