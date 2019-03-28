namespace EagleEye.Photo.Domain.CommandHandlers.Exceptions
{
    using System;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    public class PhotoAlreadyExistsException : Exception
    {
        public PhotoAlreadyExistsException([NotNull] string filename)
        {
            Helpers.Guards.Guard.NotNull(filename, nameof(filename));
            Filename = filename;
        }

        public string Filename { get; }
    }
}
