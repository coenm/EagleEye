namespace EagleEye.Core.Domain.Events
{
    using System;

    public class PersonsRemovedFromMediaItem : PersonsEventBase
    {
        public PersonsRemovedFromMediaItem(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}