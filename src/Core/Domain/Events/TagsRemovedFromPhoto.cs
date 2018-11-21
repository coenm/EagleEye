namespace EagleEye.Core.Domain.Events
{
    using System;

    using EagleEye.Core.Domain.Events.Base;

    public class TagsRemovedFromPhoto : TagsEventBase
    {
        public TagsRemovedFromPhoto(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
