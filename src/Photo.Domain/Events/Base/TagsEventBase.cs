namespace EagleEye.Photo.Domain.Events.Base
{
    using System;

    public abstract class TagsEventBase : EventBase
    {
        internal TagsEventBase(Guid id, params string[] tags)
        {
            Id = id;
            Tags = tags;
        }

        public string[] Tags { get; set; }
    }
}
