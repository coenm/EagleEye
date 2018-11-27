namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class PhotoHashUpdated : EventBase
    {
        public PhotoHashUpdated(Guid id, string hashIdentifier, byte[] hash)
        {
            Id = id;
            HashIdentifier = hashIdentifier;
            Hash = hash;
        }

        public string HashIdentifier { get; set; }

        public Memory<byte> Hash { get; set; }
    }
}