namespace EagleEye.Core
{
    public class Location
    {
        public Location()
        {
        }

        public Country Country { get; set; }

        public string City { get; set; }

        public string State { get; set; }
        public string SubLocation { get; set; }

        public Coordinates Coordinates { get; private set; }

        public void SetCoordinates(double x, double y)
        {
            Coordinates = new Coordinates(x, y);
        }
    }
}