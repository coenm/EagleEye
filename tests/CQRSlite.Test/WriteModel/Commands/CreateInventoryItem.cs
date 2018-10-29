namespace CQRSlite.Test.WriteModel.Commands
{
    using System;

    using CQRSlite.Commands;

    public class CreateInventoryItem : ICommand
    {
        public CreateInventoryItem(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }

        public string Name { get; }

        public int ExpectedVersion { get; set; }
    }
}
