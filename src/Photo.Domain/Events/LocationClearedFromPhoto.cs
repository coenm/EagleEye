namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class LocationClearedFromPhoto : EventBase
    {
        public LocationClearedFromPhoto(Guid id)
        {
            Id = id;
        }
    }
}
