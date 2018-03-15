namespace EagleEye.Core
{
    using JetBrains.Annotations;

    public class Location
    {
        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string SubLocation { get; set; }

        public Coordinate Coordinate { get; private set; }

        public void SetCoordinates(float latitude, float longitude)
        {
            Coordinate = new Coordinate(latitude, longitude);
        }

        public void SetCoordinate([NotNull] Coordinate coordinate)
        {
            Coordinate = coordinate;
        }
    }
}