namespace EagleEye.Photo.Domain.Events.Base
{
    using System;

    using CQRSlite.Events;

    public abstract class TagsEventBase : IEvent
    {
        internal TagsEventBase(Guid id, params string[] tags)
        {
            Id = id;
            Tags = tags;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string[] Tags { get; set; }
    }
}
