namespace EagleEye.Core.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    public class CreateMediaItemCommand : ICommand
    {
        public readonly string Name;

        public CreateMediaItemCommand(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }

        public int ExpectedVersion { get; set; }
    }
}