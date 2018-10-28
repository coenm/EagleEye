namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    public class LocationDto
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