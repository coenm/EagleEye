namespace Helpers.File
{
    using System;
    using System.IO;
    using System.Linq;

    using JetBrains.Annotations;

    public static class FileHelper
    {
        /// <summary>
        /// Verifies if the given path is valid according to the OS.
        /// </summary>
        /// <param name="path">path to check.</param>
        /// <returns><c>true</c> when the <paramref name="path"/> is a valid path. Otherwise <c>false</c>.</returns>
        /// <remarks>Inspired on <see href="https://stackoverflow.com/questions/422090/in-c-sharp-check-that-filename-is-possibly-valid-not-that-it-exists"/>.</remarks>
        [Pure]
        public static bool IsAbsoluteValidPath([CanBeNull] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            path = path.Trim();

            var cs = Path.GetInvalidPathChars();
            if (path.Any(x => cs.Contains(x)))
                return false;

            var isValidUri = Uri.TryCreate(path, UriKind.Absolute, out var pathUri);

            if (!isValidUri)
                return false;

            if (pathUri == null)
                return false;

            return pathUri.IsLoopback && pathUri.IsFile;
        }
    }
}
