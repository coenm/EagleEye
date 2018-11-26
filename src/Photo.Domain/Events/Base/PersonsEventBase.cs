namespace EagleEye.Photo.Domain.Events.Base
{
    using System;

    public abstract class PersonsEventBase : EventBase
    {
        internal PersonsEventBase(Guid id, params string[] persons)
        {
            Id = id;
            Persons = persons;
        }

        public string[] Persons { get; set; }
    }
}
