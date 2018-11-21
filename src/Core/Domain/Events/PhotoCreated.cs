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
            [NotNull] byte[] fileHash,
            [CanBeNull] string[] tags,
            [CanBeNull] string[] persons)
        {
            Id = id;
            Tags = tags ?? new string[0];
            Persons = persons ?? new string[0];
            FileName = filename;
            FileHash = fileHash;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; }

        public string[] Persons { get; }

        public string FileName { get; }

        public byte[] FileHash { get; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
