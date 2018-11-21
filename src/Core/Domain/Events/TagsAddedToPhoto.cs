namespace EagleEye.Core.Domain.Events
{
    using System;

    using EagleEye.Core.Domain.Events.Base;

    public class TagsAddedToPhoto : TagsEventBase
    {
        public TagsAddedToPhoto(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
