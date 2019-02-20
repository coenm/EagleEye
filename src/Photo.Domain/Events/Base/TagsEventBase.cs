namespace EagleEye.Photo.Domain.Events.Base
{
    using System;

    using JetBrains.Annotations;

    public abstract class TagsEventBase : EventBase
    {
        internal TagsEventBase(Guid id, params string[] tags)
        {
            Id = id;
            Tags = tags;
        }

        [NotNull]
        public string[] Tags { get; set; }
    }
}
