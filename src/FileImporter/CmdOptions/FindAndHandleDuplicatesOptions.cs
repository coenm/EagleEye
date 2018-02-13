using CommandLine;

namespace EagleEye.FileImporter.CmdOptions
{
    [Verb("handle-duplicates", HelpText = "Remove duplicate files (based on hash)")]
    // ReSharper disable once ClassNeverInstantiated.Global
    class FindAndHandleDuplicatesOptions
    {
        [Option("index", HelpText = "Index to compare to", Required = true)]
        public string IndexFile { get; set; }

        [Option("processing-file", HelpText = "File to find duplicates", Required = true)]
        public string ProcessingFile { get; set; }

        [Option("duplicate-action", HelpText = "Action on duplicate", Required = false, Default = FileAction.Delete)]
        public FileAction DuplicateAction { get; set; }

        [Option("duplicate-dir", HelpText = "Directory to move duplicates to (if set)", Required = false)]
        public string DuplicateDir { get; set; }

        [Option("output-new-file", HelpText = "Filename for index of new files", Required = true)]
        public string OutputNewFile { get; set; }

        [Option("output-duplicate-file", HelpText = "Filename for duplicates", Required = true)]
        public string OutputDuplicateFile { get; set; }
    }
}