namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using EagleEye.Photo.Domain.Commands.Base;
    using EagleEye.Photo.Domain.Commands.Inner;

    using JetBrains.Annotations;

    [PublicAPI]
    public class SetDateTimeTakenCommand : CommandBase
    {
        public SetDateTimeTakenCommand(Guid id, int expectedVersion, Timestamp dateTimeTaken)
            : base(id, expectedVersion)
        {
            DateTimeTaken = dateTimeTaken;
        }

        public Timestamp DateTimeTaken { get; }
    }
}
