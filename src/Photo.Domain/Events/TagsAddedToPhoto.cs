namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class TagsAddedToPhoto : TagsEventBase
    {
        public TagsAddedToPhoto(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
