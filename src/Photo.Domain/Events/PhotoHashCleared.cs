namespace EagleEye.Photo.Domain.Events
{
    using System;
    using System.Diagnostics;

    using EagleEye.Photo.Domain.Events.Base;

    public class PhotoHashCleared : EventBase
    {
        [DebuggerStepThrough]
        public PhotoHashCleared(Guid id, string hashIdentifier)
        {
            Id = id;
            HashIdentifier = hashIdentifier;
        }

        public string HashIdentifier { get; set;  }
    }
}
