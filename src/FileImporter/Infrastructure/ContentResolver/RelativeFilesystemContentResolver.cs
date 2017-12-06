using System;
using System.IO;
using FileImporter.Indexing;

namespace FileImporter.Infrastructure.ContentResolver
{
    public class RelativeFilesystemContentResolver : IContentResolver
    {
        private readonly string _baseDirectory;

        public RelativeFilesystemContentResolver(string baseDirectory)
        {
            _baseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
        }

        public Stream Read(string identifier)
        {
            return FilesystemContentResolver.Instance.Read(Path.Combine(_baseDirectory, identifier));
        }
    }
}