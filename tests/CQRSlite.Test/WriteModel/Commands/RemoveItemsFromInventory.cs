namespace CQRSlite.Test.WriteModel.Commands
{
    using System;

    using CQRSlite.Commands;

    public class RemoveItemsFromInventory : ICommand
    {
        public readonly int Count;

        public RemoveItemsFromInventory(Guid id, int count, int originalVersion)
        {
            Id = id;
            Count = count;
            ExpectedVersion = originalVersion;
        }

        public Guid Id { get; set; }

        public int ExpectedVersion { get; set; }
    }
}
