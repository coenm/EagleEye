namespace EagleEye.Photo.Domain.Events
{
    using System;
    using System.Diagnostics;

    using EagleEye.Photo.Domain.Events.Base;
    using JetBrains.Annotations;

    public class PhotoCreated : EventBase
    {
        [DebuggerStepThrough]
        public PhotoCreated(
            Guid id,
            [NotNull] string filename,
            [NotNull] string mimeType,
            [NotNull] byte[] fileHash)
        {
            Id = id;
            FileName = filename;
            MimeType = mimeType;
            FileHash = fileHash;
        }

        [NotNull] public string FileName { get; }

        [NotNull] public string MimeType { get; }

        [NotNull] public byte[] FileHash { get; }
    }
}
