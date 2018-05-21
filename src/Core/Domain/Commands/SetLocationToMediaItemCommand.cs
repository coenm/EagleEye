namespace EagleEye.Core.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    public class SetLocationToMediaItemCommand : ICommand
    {
        public SetLocationToMediaItemCommand(Guid id)
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