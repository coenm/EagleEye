namespace SearchEngine.LuceneNet.Core.Commands.UpdateIndex
{
    public class Location
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string SubLocation { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}