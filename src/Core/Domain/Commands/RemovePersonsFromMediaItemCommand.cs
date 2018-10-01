namespace EagleEye.Core.Domain.Commands
{
    using System;

    public class RemovePersonsFromMediaItemCommand : PersonsCommandBase
    {
        public RemovePersonsFromMediaItemCommand(Guid id, params string[] persons)
            : base(id, persons)
        {
        }
    }
}