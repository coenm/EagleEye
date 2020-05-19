namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using CQRSlite.Commands;
    using Dawn;
    using JetBrains.Annotations;

    [PublicAPI]
    public class CreatePhotoCommand : ICommand
    {
        public CreatePhotoCommand(
            [NotNull] string fileName,
            [NotNull] byte[] fileSha256,
            [NotNull] string photoMimeType)
        {
            Guard.Argument(fileName, nameof(fileName)).NotNull().NotWhiteSpace();
            Guard.Argument(photoMimeType, nameof(photoMimeType)).NotNull().NotWhiteSpace();
            Guard.Argument(fileSha256, nameof(fileSha256)).NotNull();

            Id = Guid.NewGuid();
            PhotoMimeType = photoMimeType;
            FileName = fileName;
            FileSha256 = fileSha256;
        }

        public Guid Id { get; set; }

        [NotNull] public string FileName { get; }

        [NotNull] public string PhotoMimeType { get; }

        [NotNull] public byte[] FileSha256 { get; }
    }
}
