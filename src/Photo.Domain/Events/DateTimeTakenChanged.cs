namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Events.Base;

    public class DateTimeTakenChanged : EventBase
    {
        public DateTimeTakenChanged(Guid id, Timestamp timestamp)
        {
            Id = id;
            DateTimeTaken = timestamp;
        }

        public Timestamp DateTimeTaken { get; }
    }
}
