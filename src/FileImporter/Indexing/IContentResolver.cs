using System.IO;

namespace EagleEye.FileImporter.Indexing
{
    public interface IContentResolver
    {
        bool Exist(string identifier);

        Stream Read(string identifier);
    }
}