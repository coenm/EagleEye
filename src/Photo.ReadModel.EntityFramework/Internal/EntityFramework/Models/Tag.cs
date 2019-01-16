namespace Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using global::Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Base;

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
