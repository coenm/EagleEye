namespace EagleEye.Core.Domain.Events
{
    using System;

    public class TagsAddedToMediaItem : TagsEventBase
    {
        public TagsAddedToMediaItem(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}