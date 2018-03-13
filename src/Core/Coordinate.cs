namespace EagleEye.Core
{
    using System;
    public class Coordinate : IEquatable<Coordinate>
    {
        public Coordinate(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public float Latitude { get; }

        public float Longitude { get; }

        public bool Equals(Coordinate other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Coordinate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
            }
        }
    }
}