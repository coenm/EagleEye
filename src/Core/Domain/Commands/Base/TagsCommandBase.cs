namespace EagleEye.Core.Domain.Commands.Base
{
    using System;

    using CQRSlite.Commands;

    public abstract class TagsCommandBase : ICommand
    {
        internal TagsCommandBase(Guid id, params string[] tags)
        {
            Id = id;
            Tags = tags;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; set; }
    }
}
