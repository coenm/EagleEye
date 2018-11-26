namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Events.Base;

    public class DateTimeTakenChanged : EventBase
    {
        public DateTimeTakenChanged(Guid id, DateTime dateTime, TimestampPrecision precision)
        {
            Id = id;
            DateTimeTaken = dateTime;
            Precision = precision;
        }

        public DateTime DateTimeTaken { get; }

        public TimestampPrecision Precision { get; }
    }
}
