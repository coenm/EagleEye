namespace Photo.ReadModel.EntityFramework.Interface.Model
{
    using JetBrains.Annotations;

    public class Location
    {
        internal Location(
            [CanBeNull] string countryCode,
            [CanBeNull] string countryName,
            [CanBeNull] string city,
            [CanBeNull] string state,
            [CanBeNull] string subLocation)
        {
            CountryCode = countryCode;
            CountryName = countryName;
            City = city;
            State = state;
            SubLocation = subLocation;
        }

        [CanBeNull] public string CountryCode { get; }

        [CanBeNull] public string CountryName { get; }

        [CanBeNull] public string City { get;  }

        [CanBeNull] public string State { get; }

        [CanBeNull] public string SubLocation { get; }

//        public Coordinate Coordinate { get; private set; }
    }
}
