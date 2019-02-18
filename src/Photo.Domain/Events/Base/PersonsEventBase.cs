namespace EagleEye.Photo.Domain.Events.Base
{
    using System;

    using JetBrains.Annotations;

    public abstract class PersonsEventBase : EventBase
    {
        internal PersonsEventBase(Guid id, params string[] persons)
        {
            Id = id;
            Persons = persons;
        }

        [NotNull] public string[] Persons { get; set; }
    }
}
