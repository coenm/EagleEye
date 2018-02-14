namespace EagleEye.FileImporter.Indexing
{
    using EagleEye.FileImporter.Imaging;

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