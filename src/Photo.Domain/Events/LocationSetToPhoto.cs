namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Events.Base;

    public class LocationSetToPhoto : EventBase
    {
        public LocationSetToPhoto(Guid id, Location location)
        {
            Id = id;
            Location = location;
        }

        public Location Location { get; }
    }
}
