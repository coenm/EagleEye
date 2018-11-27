namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal class Scores
    {
        [Key] // not sure if we want this.
        public int Id { get; set; }

        [Required]
        public Guid PhotoA { get; set; }

        [Required]
        public Guid PhotoB { get; set; }

        [Required]
        public HashIdentifiers HashIdentifier { get; set; }

        [Required]
        public float Score { get; set; }
    }
}
