namespace SearchEngine.LuceneNet.Core.Index
{
    using System.Collections.Generic;

    using SearchEngine.LuceneNet.Core.Commands.UpdateIndex;

    public class MediaResult : SearchResultBase
    {
        public MediaResult(float score)
            : base(score)
        {
        }

        public FileInformation FileInformation { get; set; }

        public List<string> Persons { get; set; }

        public List<string> Tags { get; set; }

        public Location Location { get; }

        public Timestamp DateTimeTaken { get; set; }
    }
}