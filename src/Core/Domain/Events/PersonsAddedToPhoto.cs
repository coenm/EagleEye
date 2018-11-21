namespace EagleEye.Core.Domain.Events
{
    using System;

    using EagleEye.Core.Domain.Events.Base;

    public class PersonsAddedToPhoto : PersonsEventBase
    {
        public PersonsAddedToPhoto(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}
