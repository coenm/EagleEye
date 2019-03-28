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
            Helpers.Guards.Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));

            HashIdentifier = hashIdentifier;
        }

        public string HashIdentifier { get; set; }
    }
}
