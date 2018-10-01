namespace CQRSlite.Test.Events
{
    using System;

    using CQRSlite.Events;

    public class InventoryItemRenamed : IEvent
    {
        public readonly string NewName;
 
        public InventoryItemRenamed(Guid id, string newName)
        {
            Id = id;
            NewName = newName;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}