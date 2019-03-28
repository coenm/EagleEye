﻿namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using CQRSlite.Commands;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class CreatePhotoCommand : ICommand
    {
        public CreatePhotoCommand(
            [NotNull] string fileName,
            [NotNull] byte[] fileSha256,
            [NotNull] string photoMimeType,
            [CanBeNull] string[] tags,
            [CanBeNull] string[] persons)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(fileName, nameof(fileName));
            Helpers.Guards.Guard.NotNullOrWhiteSpace(photoMimeType, nameof(photoMimeType));
            Helpers.Guards.Guard.NotNull(fileSha256, nameof(fileSha256));

            Id = Guid.NewGuid();

            PhotoMimeType = photoMimeType;
            Tags = tags;
            Persons = persons;
            FileName = fileName;
            FileSha256 = fileSha256;
        }

        public Guid Id { get; set; }

        [NotNull] public string FileName { get; }

        [NotNull] public string PhotoMimeType { get; }

        public string[] Tags { get; }

        public string[] Persons { get; }

        [NotNull] public byte[] FileSha256 { get; }
    }
}
