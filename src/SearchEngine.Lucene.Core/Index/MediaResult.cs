namespace SearchEngine.LuceneNet.Core.Index
{
    using System.Collections.Generic;

    using SearchEngine.Interface.Commands.ParameterObjects;

    public class MediaResult : SearchResultBase
    {
        public MediaResult(float score)
            : base(score)
        {
        }

        public FileInformation FileInformation { get; set; }

        public List<string> Persons { get; set; }

        public List<string> Tags { get; set; }

        public Location Location { get; set; }

        public Timestamp DateTimeTaken { get; set; }
    }
}