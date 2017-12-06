using FileImporter.Imaging;

namespace FileImporter.Indexing
{
    public class FileIndex
    {
        public FileIndex(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ImageHashValues Hashes { get; set; }
    }
}