namespace EagleEye.Photo.Domain.Commands
{
    using System;
    using EagleEye.Photo.Domain.Commands.Base;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [PublicAPI]
    public class UpdatePhotoHashCommand : CommandBase
    {
        public UpdatePhotoHashCommand(Guid id, int expectedVersion, [NotNull] string hashIdentifier, [NotNull] byte[] fileHash)
            : base(id, expectedVersion)
        {
            Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));
            Guard.NotNull(fileHash, nameof(fileHash));

            HashIdentifier = hashIdentifier;
            FileHash = fileHash;
        }

        public string HashIdentifier { get; set; }

        public byte[] FileHash { get; }
    }
}