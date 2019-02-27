namespace EagleEye.Photo.Domain.Events
{
    using System;
    using System.Diagnostics;

    using EagleEye.Photo.Domain.Events.Base;

    public class PhotoHashAdded : EventBase
    {
        [DebuggerStepThrough]
        public PhotoHashAdded(Guid id, string hashIdentifier, ulong hash)
        {
            Id = id;
            HashIdentifier = hashIdentifier;
            Hash = hash;
        }

        public string HashIdentifier { get; set; }

        public ulong Hash { get; set; }
    }
}
