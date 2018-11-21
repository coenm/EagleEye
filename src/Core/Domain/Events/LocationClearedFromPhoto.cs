namespace EagleEye.Core.Domain.Events
{
    using System;

    using CQRSlite.Events;

    public class LocationClearedFromPhoto : IEvent
    {
        public LocationClearedFromPhoto(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}