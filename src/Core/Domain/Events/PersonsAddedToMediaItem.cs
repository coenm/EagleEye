namespace EagleEye.Core.Domain.Events
{
    using System;

    public class PersonsAddedToMediaItem : PersonsEventBase
    {
        public PersonsAddedToMediaItem(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}