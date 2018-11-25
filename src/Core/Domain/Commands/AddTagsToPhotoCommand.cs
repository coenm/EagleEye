namespace EagleEye.Core.Domain.Commands
{
    using System;
    using EagleEye.Core.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class AddTagsToPhotoCommand : TagsCommandBase
    {
        public AddTagsToPhotoCommand(Guid id, params string[] tags)
            : base(id, tags)
        {
        }
    }
}
