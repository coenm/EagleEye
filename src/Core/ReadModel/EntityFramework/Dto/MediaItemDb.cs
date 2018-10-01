namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    using System.ComponentModel.DataAnnotations;

    public class MediaItemDb : VersionedDb
    {
        [Required]
        [MaxLength(256)] // dummy, but demonstrates possibilites
        public string Filename { get; set; }

        // for now, the easiest solution is to serialize the data and store it as a string.
        // why not? ;-)
        [Required]
        public string SerializedMediaItemDto { get; set; }
    }

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