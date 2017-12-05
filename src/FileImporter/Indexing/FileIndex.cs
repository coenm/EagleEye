using FileImporter.Imaging;

namespace FileImporter.Indexing
{
    public struct FileIndex
    {
        public string Identifier { get; set; }

        public ImageHashValues Hashes { get; set; }
    }
}