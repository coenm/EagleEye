﻿namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using Dawn;
    using EagleEye.Photo.Domain.Commands.Base;
    using JetBrains.Annotations;

    [PublicAPI]
    public class UpdatePhotoHashCommand : CommandBase
    {
        public UpdatePhotoHashCommand(Guid id, int? expectedVersion, [NotNull] string hashIdentifier, ulong fileHash)
            : base(id, expectedVersion)
        {
            Guard.Argument(hashIdentifier, nameof(hashIdentifier)).NotNull().NotWhiteSpace();

            HashIdentifier = hashIdentifier;
            PhotoHash = fileHash;
        }

        public string HashIdentifier { get; set; }

        public ulong PhotoHash { get; }
    }
}
