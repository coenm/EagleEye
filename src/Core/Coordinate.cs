namespace EagleEye.Core
{
    public class Coordinate
    {
        public Coordinate(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public float Latitude { get; }

        public float Longitude { get; }
    }
}