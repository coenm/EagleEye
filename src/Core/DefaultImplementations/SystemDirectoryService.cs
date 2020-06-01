namespace EagleEye.Core.DefaultImplementations
{
    using System.Collections.Generic;
    using System.IO;

    using EagleEye.Core.Interfaces.Core;

    public sealed class SystemDirectoryService : IDirectoryService
    {
        private SystemDirectoryService()
        {
        }

        public static SystemDirectoryService Instance { get; } = new SystemDirectoryService();

        public bool Exists(string path) => Directory.Exists(path);

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFiles(path, searchPattern, searchOption);
    }
}
