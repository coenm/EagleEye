namespace EagleEye.Photo.Domain.Commands.Base
{
    using System;

    using CQRSlite.Commands;

    public abstract class CommandBase : ICommand
    {
        public CommandBase(Guid id, int expectedVersion)
        {
            Id = id;
            ExpectedVersion = expectedVersion;
        }

        public Guid Id { get; set;  }

        public int ExpectedVersion { get; set; }
    }
}
