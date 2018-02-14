namespace EagleEye.FileImporter.Indexing
{
    using System.IO;

    public interface IContentResolver
    {
        bool Exist(string identifier);

        Stream Read(string identifier);
    }
}