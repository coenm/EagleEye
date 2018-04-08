namespace EagleEye.Core.Domain.Events
{
    using System;

    public class TagsRemovedFromMediaItem : TagsEventBase
    {
        public TagsRemovedFromMediaItem(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}