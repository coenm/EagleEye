namespace EagleEye.FileImporter.Infrastructure.ContentResolver
{
    using System;
    using System.IO;

    using EagleEye.FileImporter.Indexing;

    public class RelativeFilesystemContentResolver : IContentResolver
    {
        private readonly string baseDirectory;

        public RelativeFilesystemContentResolver(string baseDirectory)
        {
            this.baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        }

        public bool Exist(string identifier)
        {
            return FilesystemContentResolver.Instance.Exist(FullPath(identifier));
        }

        public Stream Read(string identifier)
        {
            return FilesystemContentResolver.Instance.Read(FullPath(identifier));
        }

        private string FullPath(string identifier)
        {
            return Path.Combine(baseDirectory, identifier);
        }
    }
}