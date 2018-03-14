namespace EagleEye.Core.Interfaces
{
    using System.IO;

    using JetBrains.Annotations;

    public interface IFileService
    {
        bool FileExists([NotNull] string filename);

        Stream OpenRead([NotNull] string filename);
    }
}