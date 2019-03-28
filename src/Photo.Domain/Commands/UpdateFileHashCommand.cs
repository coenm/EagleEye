namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class UpdateFileHashCommand : CommandBase
    {
        public UpdateFileHashCommand(Guid id, int expectedVersion, [NotNull] byte[] fileHash)
        : base(id, expectedVersion)
        {
            Dawn.Guard.Argument(fileHash, nameof(fileHash)).NotNull();

            FileHash = fileHash;
        }

        public byte[] FileHash { get; }
    }
}
