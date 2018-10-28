namespace CQRSlite.Test.Events
{
    using System;

    using CQRSlite.Events;

    public class ItemsCheckedInToInventory : IEvent
    {
        public ItemsCheckedInToInventory(Guid id, int count)
        {
            Id = id;
            Count = count;
        }

        public Guid Id { get; set; }

        public int Count { get; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
