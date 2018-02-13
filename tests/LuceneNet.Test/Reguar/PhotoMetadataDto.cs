using System.Collections.Generic;
using System.Linq;

namespace EagleEye.LuceneNet.Test.Reguar
{
    public class PhotoMetadataDto
    {
        public PhotoMetadataDto()
        {
        }

        public PhotoMetadataDto(string filename, params string[] persons)
        {
            Filename = filename;
            Persons = persons?.ToList() ?? new List<string>();
        }

        public string Filename { get; set; }
        public List<string> Persons { get; set; }
    }
}