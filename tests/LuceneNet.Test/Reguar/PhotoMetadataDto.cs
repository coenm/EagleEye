using System.Collections.Generic;
using System.Linq;

namespace LuceneNet.Test.Reguar
{
    public class PhotoMetadataDto
    {
        public PhotoMetadataDto()
        {
            Persons = new List<string>();
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