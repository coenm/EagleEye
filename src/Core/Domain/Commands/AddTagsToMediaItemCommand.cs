namespace EagleEye.Core.Domain.Commands
{
    using System;

    public class AddTagsToMediaItemCommand : TagsCommandBase
    {
        public AddTagsToMediaItemCommand(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}