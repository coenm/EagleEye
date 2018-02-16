namespace EagleEye.Core
{
    public class Country
    {
        public Country(string countryCode, string countryName)
        {
            CountryCode = countryCode;
            CountryName = countryName;
        }

        public string CountryCode { get; }

        public string CountryName { get; }
    }
}