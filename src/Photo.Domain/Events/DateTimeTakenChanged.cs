namespace EagleEye.Photo.Domain.Events
{
    using System;

    using CQRSlite.Events;

    using EagleEye.Photo.Domain.Aggregates;

    public class DateTimeTakenChanged : IEvent
    {
        public DateTimeTakenChanged(Guid id, DateTime dateTime, TimestampPrecision precision)
        {
            Id = id;
            DateTimeTaken = dateTime;
            Precision = precision;
        }

        public DateTime DateTimeTaken { get; }

        public TimestampPrecision Precision { get; }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
