﻿namespace EagleEye.FileImporter.CmdOptions
{
    using CommandLine;
    using JetBrains.Annotations;

    [Verb("lucene", HelpText = "demo lucene read model search.")]
    [UsedImplicitly]
    internal class DemoLuceneSearchOptions
    {
        [Option(shortName: 'd', longName: "directory", HelpText = "Directory to find media files.", Required = true)]
        public string Directory { get; set; }
    }
}
