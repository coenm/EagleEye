using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LuceneNet.Test.Persons
{

    public class SearchResultPhotoMetadataDto : PhotoMetadataDto
    {
        public SearchResultPhotoMetadataDto()
        {
        }

        public SearchResultPhotoMetadataDto(string filename, params string[] persons) : base(filename, persons)
        {
        }

        public float Score { get; set; }
    }

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