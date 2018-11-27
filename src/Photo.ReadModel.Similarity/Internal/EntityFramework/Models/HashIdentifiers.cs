namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal class HashIdentifiers
    {
        [Key]
        public Guid Id { get; set; }

        [Required] // unique
        [MinLength(2)]
        [MaxLength(100)]
        public string HashIdentifier { get; set; }
    }
}
