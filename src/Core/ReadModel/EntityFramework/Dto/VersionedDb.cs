namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public abstract class VersionedDb
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Version { get; set; }

        [Required]
        public DateTimeOffset TimeStampUtc { get; set; }
    }
}