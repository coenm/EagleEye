using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using FileImporter.Imaging;
using FileImporter.Json;

namespace FileImporter.Search
{

    public class FindImage /* : IQuery */
    {

    }

    public class FoundImages
    {

        public int Take { get; set; }
        public int Skip { get; set; }

        public int TotalResult { get; set; }
    }



    public class ImageInfo
    {
        public string Filename { get; set; }
        public ImageHashValues Hashes { get; set; }
    }

};