namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class SetLocationToPhotoCommand : CommandBase
    {
        public SetLocationToPhotoCommand(Guid id, int expectedVersion)
            : base(id, expectedVersion)
        {
        }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string SubLocation { get; set; }

        public float? Latitude { get; }

        public float? Longitude { get; }
    }
}
