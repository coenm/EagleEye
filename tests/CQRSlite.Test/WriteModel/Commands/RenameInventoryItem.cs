namespace CQRSlite.Test.WriteModel.Commands
{
    using System;

    using CQRSlite.Commands;

    public class RenameInventoryItem : ICommand
    {
        public RenameInventoryItem(Guid id, string newName, int originalVersion)
        {
            Id = id;
            NewName = newName;
            ExpectedVersion = originalVersion;
        }

        public Guid Id { get; set; }

        public string NewName { get; }

        public int ExpectedVersion { get; set; }
    }
}
