namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class TagsRemovedFromPhoto : TagsEventBase
    {
        public TagsRemovedFromPhoto(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
