namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class PhotoHashCleared : EventBase
    {
        public PhotoHashCleared(Guid id, string hashIdentifier)
        {
            Id = id;
            HashIdentifier = hashIdentifier;
        }

        public string HashIdentifier { get; set;  }
    }
}