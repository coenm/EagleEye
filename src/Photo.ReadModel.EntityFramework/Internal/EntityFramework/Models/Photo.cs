namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Base;
    using JetBrains.Annotations;

    internal class Photo : VersionedItemBase
    {
        [Required]
        [MinLength(2)]
        [MaxLength(1024)]
        public string Filename { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FileMimeType { get; set; }

        [Required]
        public DateTimeOffset EventTimestamp { get; set; }

        [Required]
        public byte[] FileSha256 { get; set; }

/*
        [Required]
        public uint FileSize { get; set; }
*/

        [CanBeNull]
        public List<Tag> Tags { get; set; }

        [CanBeNull]
        public List<Person> People { get; set; }

        [CanBeNull]
        public Location Location { get; set; }

        public DateTime? DateTimeTaken { get; set; }
    }
}
