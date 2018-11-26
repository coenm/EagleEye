namespace EagleEye.Photo.Domain.Commands.Base
{
    using System;

    using CQRSlite.Commands;

    public abstract class PersonsCommandBase : ICommand
    {
        internal PersonsCommandBase(Guid id, params string[] persons)
        {
            Id = id;
            Persons = persons;
        }

        public Guid Id { get; set; }

        public string[] Persons { get; set; }
    }
}
