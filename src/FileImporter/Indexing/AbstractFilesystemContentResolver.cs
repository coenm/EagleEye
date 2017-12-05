using System.IO;

namespace FileImporter.Indexing
{
    public class AbstractFilesystemContentResolver : IContentResolver
    {
        private readonly string _baseDirectory;

        public AbstractFilesystemContentResolver(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public Stream Read(string identifier)
        {
            return FilesystemContentResolver.Instance.Read(Path.Combine(_baseDirectory, identifier));
        }
    }
}