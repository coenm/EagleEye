namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class FileHashUpdated : EventBase
    {
        public FileHashUpdated(Guid id, byte[] hash)
        {
            Id = id;
            Hash = hash;
        }

        public Memory<byte> Hash { get; set; }
    }
}