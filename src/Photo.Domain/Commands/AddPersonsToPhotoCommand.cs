namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;

    using JetBrains.Annotations;

    [PublicAPI]
    public class AddPersonsToPhotoCommand : PersonsCommandBase
    {
        public AddPersonsToPhotoCommand(Guid id, int expectedVersion, params string[] persons)
            : base(id, expectedVersion, persons)
        {
        }
    }
}
