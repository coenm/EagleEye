namespace EagleEye.EventStore.NEventStoreAdapter.Test
{
    using System;

    using CQRSlite.Events;

    public class DummyEvent : IEvent
    {
        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public static DummyEvent Create(Guid guid, int version, DateTimeOffset timestamp)
        {
            return new DummyEvent
                   {
                       Version = version,
                       Id = guid,
                       TimeStamp = timestamp,
                   };
        }
    }
}