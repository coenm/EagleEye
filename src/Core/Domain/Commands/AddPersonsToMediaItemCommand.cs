namespace EagleEye.Core.Domain.Commands
{
    using System;

    public class AddPersonsToMediaItemCommand : PersonsCommandBase
    {
        public AddPersonsToMediaItemCommand(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}