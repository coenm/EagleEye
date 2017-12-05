using System.IO;

namespace FileImporter.Indexing
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