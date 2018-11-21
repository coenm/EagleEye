namespace EagleEye.Core.Domain.Events
{
    using System;

    using EagleEye.Core.Domain.Events.Base;

    public class PersonsRemovedFromPhoto : PersonsEventBase
    {
        public PersonsRemovedFromPhoto(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}
