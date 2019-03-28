namespace EagleEye.Photo.Domain.CommandHandlers.Exceptions
{
    using System;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    public class PhotoAlreadyExistsException : Exception
    {
        public PhotoAlreadyExistsException([NotNull] string filename)
        {
            Dawn.Guard.Argument(filename, nameof(filename)).NotNull();
            Filename = filename;
        }

        public string Filename { get; }
    }
}
