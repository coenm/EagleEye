namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class MediaItemDb : VersionedDb
    {
        [Required]
        [MaxLength(256)] // dummy, but demonstrates possibilites
        public string Filename { get; set; }

        // for now, the easiest solution is to serialize the data and store it as a string.
        // why not? ;-)
        [Required]
        public string SerializedMediaItemDto { get; set; }
    }


    public class MediaItemDto
    {
        public List<string> Tags { get; set; }
        public List<string> Persons { get; set; }
    }
}