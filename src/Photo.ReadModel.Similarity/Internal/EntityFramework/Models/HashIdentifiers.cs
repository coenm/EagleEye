namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    internal class HashIdentifiers
    {
        [Key]
        public int Id { get; set; }

        [Required] // unique
        [MinLength(2)]
        [MaxLength(100)]
        public string HashIdentifier { get; set; }
    }
}
