using CommandLine;

namespace FileImporter
{
    class Options
    {
        [Option('i', "inputdir", Required = true, HelpText = "Input directory to process.")]
        public string InputDirectory { get; set; }


        [Option('f', "inputfile", Required = true, HelpText = "Input file containing already processed files.")]
        public string InputFile { get; set; }

        
        [Option('o', "outputdir", Required = true, HelpText = "Output directory to move already processed files to.")]
        public string OutputDirectory { get; set; }
    }
}