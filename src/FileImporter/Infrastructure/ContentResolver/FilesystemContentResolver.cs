using System.IO;
using FileImporter.Indexing;

namespace FileImporter.Infrastructure.ContentResolver
{
    public class FilesystemContentResolver : IContentResolver
    {
        public static readonly FilesystemContentResolver Instance = new FilesystemContentResolver();

        private FilesystemContentResolver()
        {
        }

        public Stream Read(string identifier)
        {
            return File.OpenRead(identifier);
        }
    }
}