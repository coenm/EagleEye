namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal class Scores
    {
        [Required]
        public Guid PhotoA { get; set; }

        [Required]
        public int VersionPhotoA { get; set; }

        [Required]
        public Guid PhotoB { get; set; }

        [Required]
        public int VersionPhotoB { get; set; }

        [Required]
        public int HashIdentifierId { get; set; }

        public HashIdentifiers HashIdentifier { get; set; }

        [Required]
        public double Score { get; set; }
    }
}
