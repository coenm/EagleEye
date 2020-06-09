namespace EagleEye.FileImporter.Infrastructure.ContentResolver
{
    using System.IO;

    using Dawn;
    using EagleEye.Core.DefaultImplementations;
    using EagleEye.Core.Interfaces.Core;

    public class RelativeSystemFileService : IFileService
    {
        private readonly string baseDirectory;

        public RelativeSystemFileService(string baseDirectory)
        {
            Guard.Argument(baseDirectory, nameof(baseDirectory)).NotNull();
            this.baseDirectory = baseDirectory;
        }

        public bool FileExists(string filename)
        {
            return SystemFileService.Instance.FileExists(FullPath(filename));
        }

        public Stream OpenRead(string filename)
        {
            return SystemFileService.Instance.OpenRead(FullPath(filename));
        }

        public Stream OpenWrite(string filename)
        {
            return SystemFileService.Instance.OpenWrite(FullPath(filename));
        }

        private string FullPath(string identifier)
        {
            return Path.Combine(baseDirectory, identifier);
        }
    }
}
