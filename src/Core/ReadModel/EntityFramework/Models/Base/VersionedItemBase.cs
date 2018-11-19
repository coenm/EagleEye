namespace EagleEye.Core.ReadModel.EntityFramework.Models.Base
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public abstract class VersionedItemBase : ValueObjectItemBase
    {
        [Required]
        public int Version { get; set; }

//        [Required]
//        [Timestamp]
//        public byte[] Timestamp { get; set; }
    }
}
