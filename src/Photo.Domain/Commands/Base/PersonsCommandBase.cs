namespace EagleEye.Photo.Domain.Commands.Base
{
    using System;

    public abstract class PersonsCommandBase : CommandBase
    {
        internal PersonsCommandBase(Guid id, int expectedVersion, params string[] persons)
        : base(id, expectedVersion)
        {
            Persons = persons;
        }

        public string[] Persons { get; set; }
    }
}
