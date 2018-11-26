namespace Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using global::Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models.Base;

    internal class Location : ValueObjectItemBase
    {
        [MaxLength(5)]
        public string CountryCode { get; set; }

        [MaxLength(100)]
        public string CountryName { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string SubLocation { get; set; }

        public float? Longitude { get; set; }

        public float? Latitude { get; set; }
    }
}
