namespace EagleEye.Photo.Domain.Aggregates.SnapshotDtos
{
    internal class LocationSnapshot
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string SubLocation { get; set; }

        public float? Longitude { get; set; }

        public float? Latitude { get; set; }
    }
}
