using System.IO;

namespace FileImporter.Indexing
{
    public interface IContentResolver
    {
        Stream Read(string identifier);
    }
}