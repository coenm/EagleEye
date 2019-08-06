namespace EagleEye.FileImporter.Infrastructure.ContentResolver
{
    using System.IO;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.FileImporter.Indexing;

    public class RelativeFilesystemContentResolver : IContentResolver, IFileService
    {
        private readonly string baseDirectory;

        public RelativeFilesystemContentResolver(string baseDirectory)
        {
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull();
            this.baseDirectory = baseDirectory;
        }

        public bool Exist(string identifier)
        {
            return FilesystemContentResolver.Instance.Exist(FullPath(identifier));
        }

        private string FullPath(string identifier)
        {
            return Path.Combine(baseDirectory, identifier);
        }

        public bool FileExists(string filename)
        {
            return Exist(filename);
        }

        public Stream OpenRead(string filename)
        {
            return File.OpenRead(FullPath(filename));
        }
    }
}
