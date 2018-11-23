namespace SearchEngine.LuceneNet.ReadModel.Interface.Model
{
    using JetBrains.Annotations;

    public class Location
    {
        private Location(
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


        [CanBeNull]
        internal static Location Create(
            [CanBeNull] string countryCode,
            [CanBeNull] string countryName,
            [CanBeNull] string city,
            [CanBeNull] string state,
            [CanBeNull] string subLocation)
        {
            var allNull = true;
            allNull &= countryCode == null;
            allNull &= countryName == null;
            allNull &= city == null;
            allNull &= state == null;
            allNull &= subLocation == null;

            if (allNull)
                return null;

            return new Location(countryCode, countryName, city, state, subLocation);
        }
    }
}
