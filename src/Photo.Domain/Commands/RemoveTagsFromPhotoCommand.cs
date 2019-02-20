namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class RemoveTagsFromPhotoCommand : TagsCommandBase
    {
        public RemoveTagsFromPhotoCommand(Guid id, int expectedVersion, params string[] tags)
            : base(id, expectedVersion, tags)
        {
        }
    }
}
