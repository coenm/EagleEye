using CommandLine;

namespace FileImporter.CmdOptions
{
    [Verb("search-file")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class SearchDuplicateFileOptions
    {
        [Option('f', "File", HelpText = "Original image", Required = true)]
        public string OriginalImageFile { get; set; }

        [Option('i', "index-file", HelpText = "Filename containing all indexes", Required = true)]
        public string IndexFile { get; set; }
    }
}