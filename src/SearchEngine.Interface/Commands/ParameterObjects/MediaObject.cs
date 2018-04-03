namespace SearchEngine.Interface.Commands.ParameterObjects
{
    using System.Collections.Generic;

    public class MediaObject
    {
        public FileInformation FileInformation { get; set; }

        public List<string> Persons { get; set; }

        public List<string> Tags { get; set; }

        public Location Location { get; set; }

        public Timestamp DateTimeTaken { get; set; }
    }
}