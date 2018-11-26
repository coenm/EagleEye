namespace EagleEye.Photo.Domain.Events
{
    using System;

    using EagleEye.Photo.Domain.Events.Base;

    public class PersonsAddedToPhoto : PersonsEventBase
    {
        public PersonsAddedToPhoto(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}
