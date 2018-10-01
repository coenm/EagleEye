namespace CQRSlite.Test.WriteModel.Commands
{
    using System;

    using CQRSlite.Commands;

    public class CreateInventoryItem : ICommand
	{
        public readonly string Name;

        public CreateInventoryItem(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }

        public int ExpectedVersion { get; set; }
	}
}