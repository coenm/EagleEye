namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Base
{
    using System.ComponentModel.DataAnnotations;

    internal abstract class VersionedItemBase : ValueObjectItemBase
    {
        [Required]
        public int Version { get; set; }

/*
        [Required]
        [Timestamp]
        public byte[] Timestamp { get; set; }
*/
    }
}
