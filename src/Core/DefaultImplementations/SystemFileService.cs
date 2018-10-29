namespace EagleEye.Core.DefaultImplementations
{
    using System.IO;

    using EagleEye.Core.Interfaces;

    public class SystemFileService : IFileService
    {
        private SystemFileService()
        {
        }

        public static SystemFileService Instance { get; } = new SystemFileService();

        public Stream OpenRead(string filename)
        {
            return File.OpenRead(filename);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }
    }
}