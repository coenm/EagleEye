using EagleEye.FileImporter.Imaging;

namespace EagleEye.FileImporter.Indexing
{
    public class ImageData
    {
        public ImageData(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ImageHashValues Hashes { get; set; }
    }
}