﻿namespace EagleEye.Photo.Domain.CommandHandlers.Exceptions
{
    using System;

    using Dawn;
    using JetBrains.Annotations;

    public class PhotoAlreadyExistsException : Exception
    {
        public PhotoAlreadyExistsException([NotNull] string filename)
            : base($@"Photo with filename '{filename}' already exists.")
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
            Filename = filename;
        }

        public string Filename { get; }
    }
}
