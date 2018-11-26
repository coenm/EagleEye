namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class PersonsRemovedFromPhoto : PersonsEventBase
    {
        public PersonsRemovedFromPhoto(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}
