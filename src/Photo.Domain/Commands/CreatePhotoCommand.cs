namespace EagleEye.Photo.Domain.Commands
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
            Dawn.Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();
            Dawn.Guard.Argument(photoMimeType, nameof(photoMimeType)).NotNull().NotEmpty();
            Dawn.Guard.Argument(fileSha256, nameof(fileSha256)).NotNull();

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
