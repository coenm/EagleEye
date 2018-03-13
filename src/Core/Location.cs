namespace EagleEye.Core
{
    using System.Drawing;

    public class Location
    {
        public Location()
        {
        }

        public Country Country { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string SubLocation { get; set; }

        public Coordinate Coordinate { get; private set; }

        public void SetCoordinates(float latitude, float longitude)
        {
            Coordinate = new Coordinate(latitude, longitude);
        }
    }
}