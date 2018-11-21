namespace EagleEye.Core.Domain.Commands
{
    using System;

    using CQRSlite.Commands;
    using JetBrains.Annotations;

    [PublicAPI]
    public class CreatePhotoCommand : ICommand
    {
        public CreatePhotoCommand(
            [NotNull] string name,
            [NotNull] byte[] fileSha256,
            [CanBeNull] string[] tags,
            [CanBeNull] string[] persons)
        {
            Id = Guid.NewGuid();
            Tags = tags;
            Persons = persons;
            Name = name;
            FileSha256 = fileSha256;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; set; }

        public string[] Persons { get; set; }

        public string Name { get; }

        public byte[] FileSha256 { get; }

        public int ExpectedVersion { get; set; }
    }
}
