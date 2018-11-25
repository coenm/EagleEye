namespace EagleEye.Core.Domain.Events
{
    using System;
    using System.Diagnostics;

    using CQRSlite.Events;
    using JetBrains.Annotations;

    public class PhotoCreated : IEvent
    {
        [DebuggerStepThrough]
        public PhotoCreated(
            Guid id,
            [NotNull] string filename,
            [NotNull] string mimeType,
            [NotNull] byte[] fileHash,
            [CanBeNull] string[] tags,
            [CanBeNull] string[] persons)
        {
            Id = id;
            FileName = filename;
            MimeType = mimeType;
            FileHash = fileHash;
            Tags = tags ?? new string[0];
            Persons = persons ?? new string[0];
        }

        public Guid Id { get; set; }

        [NotNull] public string FileName { get; }

        [NotNull] public string MimeType { get; }

        [NotNull] public byte[] FileHash { get; }

        public string[] Tags { get; }

        public string[] Persons { get; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
