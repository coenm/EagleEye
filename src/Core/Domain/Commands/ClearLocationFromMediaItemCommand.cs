namespace EagleEye.Core.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    public class ClearLocationFromMediaItemCommand : ICommand
    {
        public ClearLocationFromMediaItemCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}