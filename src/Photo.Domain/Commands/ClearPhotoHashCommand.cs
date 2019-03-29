namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class ClearPhotoHashCommand : CommandBase
    {
        public ClearPhotoHashCommand(Guid id, int expectedVersion, [NotNull] string hashIdentifier)
            : base(id, expectedVersion)
        {
            Dawn.Guard.Argument(hashIdentifier, nameof(hashIdentifier)).NotNull().NotWhiteSpace();

            HashIdentifier = hashIdentifier;
        }

        public string HashIdentifier { get; set; }
    }
}
