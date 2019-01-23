namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models.Base
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal abstract class ValueObjectItemBase
    {
        [Required]
        public Guid Id { get; set; }
    }
}
