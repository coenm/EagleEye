namespace EagleEye.Core.Domain.Events.Base
{
    using System;

    using CQRSlite.Events;

    public abstract class PersonsEventBase : IEvent
    {
        internal PersonsEventBase(Guid id, params string[] persons)
        {
            Id = id;
            Persons = persons;
        }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string[] Persons { get; set; }
    }
}
