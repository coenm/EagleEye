namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Base;

    /// <summary>
    /// Value object.
    /// </summary>
    internal class Tag : ValueObjectItemBase
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Value { get; set; }
    }
}
