namespace EagleEye.Photo.Domain.Commands.Base
{
    using System;

    public abstract class TagsCommandBase : CommandBase
    {
        internal TagsCommandBase(Guid id, int? expectedVersion, params string[] tags)
            : base(id, expectedVersion)
        {
            Tags = tags;
        }

        public string[] Tags { get; set; }
    }
}
