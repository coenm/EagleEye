namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class ClearLocationFromPhotoCommand : CommandBase
    {
        public ClearLocationFromPhotoCommand(Guid id, int expectedVersion)
            : base(id, expectedVersion)
        {
        }
    }
}
