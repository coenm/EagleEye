namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    using EagleEye.Photo.Domain.Commands.Inner;

    using JetBrains.Annotations;

    [PublicAPI]
    public class SetDateTimeTakenCommand : ICommand
    {
        public SetDateTimeTakenCommand(Guid id, Timestamp dateTimeTaken)
        {
            Id = id;
            DateTimeTaken = dateTimeTaken;
        }

        public Guid Id { get; }

        public Timestamp DateTimeTaken { get; }
    }
}
