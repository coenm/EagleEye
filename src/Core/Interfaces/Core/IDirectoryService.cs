namespace EagleEye.Core.Interfaces.Core
{
    using System.Collections.Generic;
    using System.IO;

    using JetBrains.Annotations;

    public interface IDirectoryService
    {
        bool Exists([NotNull] string directory);

        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    }
}
