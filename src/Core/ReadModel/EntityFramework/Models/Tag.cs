namespace EagleEye.Core.ReadModel.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Core.ReadModel.EntityFramework.Models.Base;

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
