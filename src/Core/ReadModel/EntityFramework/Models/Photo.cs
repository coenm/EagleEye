namespace EagleEye.Core.ReadModel.EntityFramework.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Core.ReadModel.EntityFramework.Models.Base;

    public class Photo : VersionedItemBase
    {
        [Required]
        [MinLength(2)]
        [MaxLength(1024)]
        public string Filename { get; set; }

        [Required]
        public DateTimeOffset EventTimestamp { get; set; }

        [Required]
        public byte[] FileSha256 { get; set; }

//        [Required]
//        public uint FileSize { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Person> People { get; set; }

        public Location Location { get; set; }
    }
}
