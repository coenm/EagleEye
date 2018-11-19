namespace EagleEye.Core.ReadModel.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Core.ReadModel.EntityFramework.Models.Base;

    /// <summary>
    /// Value object.
    /// </summary>
    public class Person : ValueObjectItemBase
    {
        // Index and is unique set using fluent API.
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
