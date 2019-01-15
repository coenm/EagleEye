namespace EagleEye.Photo.Domain.Events
{
    using System;
    using System.Diagnostics;

    using EagleEye.Photo.Domain.Events.Base;

    public class PhotoHashUpdated : EventBase
    {
        [DebuggerStepThrough]
        public PhotoHashUpdated(Guid id, string hashIdentifier, byte[] hash)
        {
            Id = id;
            HashIdentifier = hashIdentifier;
            Hash = hash;
        }

        public string HashIdentifier { get; set; }

        public byte[] Hash { get; set; }
    }
}
