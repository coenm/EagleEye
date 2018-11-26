namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    using JetBrains.Annotations;

    [PublicAPI]
    public class SetLocationToPhotoCommand : ICommand
    {
        public SetLocationToPhotoCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string SubLocation { get; set; }

        public float? Latitude { get; }

        public float? Longitude { get; }
    }
}
