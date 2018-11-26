namespace EagleEye.Photo.Domain.Aggregates
{
    using System;

    public class Location
    {
        public Location(
            string countryCode,
            string countryName,
            string state,
            string city,
            string subLocation,
            float? longitude,
            float? latitude)
        {
            if (longitude == null && latitude != null)
                throw new ArgumentException("Longitude and Latitude must be both null or both have a value.");
            if (longitude != null && latitude == null)
                throw new ArgumentException("Longitude and Latitude must be both null or both have a value.");

            CountryCode = countryCode;
            CountryName = countryName;
            State = state;
            City = city;
            SubLocation = subLocation;
            Longitude = longitude;
            Latitude = latitude;
        }

        public string CountryCode { get; private set; }

        public string CountryName { get; private set; }

        public string State { get; private set; }

        public string City { get; private set; }

        public string SubLocation { get; private set; }

        public float? Longitude { get; private set; }

        public float? Latitude { get; private set; }
    }
}
