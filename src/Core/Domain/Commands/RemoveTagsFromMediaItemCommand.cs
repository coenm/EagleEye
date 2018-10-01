namespace EagleEye.Core.Domain.Commands
{
    using System;

    public class RemoveTagsFromMediaItemCommand : TagsCommandBase
    {
        public RemoveTagsFromMediaItemCommand(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}