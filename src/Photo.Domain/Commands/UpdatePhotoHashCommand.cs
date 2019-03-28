﻿namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class UpdatePhotoHashCommand : CommandBase
    {
        public UpdatePhotoHashCommand(Guid id, int expectedVersion, [NotNull] string hashIdentifier, ulong fileHash)
            : base(id, expectedVersion)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));

            HashIdentifier = hashIdentifier;
            PhotoHash = fileHash;
        }

        public string HashIdentifier { get; set; }

        public ulong PhotoHash { get; }
    }
}
