namespace EagleEye.Photo.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    using JetBrains.Annotations;

    [PublicAPI]
    public class ClearLocationFromPhotoCommand : ICommand
    {
        public ClearLocationFromPhotoCommand(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}
