namespace EagleEye.Core.Domain.Events
{
    using System;

    using CQRSlite.Events;

    public class MediaItemCreated : IEvent
    {
        public readonly string Name;

        public MediaItemCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}