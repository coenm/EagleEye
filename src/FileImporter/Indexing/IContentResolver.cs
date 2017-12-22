using System.IO;

namespace FileImporter.Indexing
{
    public interface IContentResolver
    {
        bool Exist(string identifier);

        Stream Read(string identifier);
    }
}