namespace EagleEye.FileImporter.Infrastructure.ContentResolver
{
    using System.IO;

    using EagleEye.FileImporter.Indexing;
    using Helpers.Guards; using Dawn;

    public class RelativeFilesystemContentResolver : IContentResolver
    {
        private readonly string baseDirectory;

        public RelativeFilesystemContentResolver(string baseDirectory)
        {
            Helpers.Guards.Guard.NotNull(baseDirectory, nameof(baseDirectory));
            this.baseDirectory = baseDirectory;
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
