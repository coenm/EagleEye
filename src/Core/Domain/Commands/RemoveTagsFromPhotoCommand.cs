namespace EagleEye.Core.Domain.Commands
{
    using System;

    using EagleEye.Core.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class RemoveTagsFromPhotoCommand : TagsCommandBase
    {
        public RemoveTagsFromPhotoCommand(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
