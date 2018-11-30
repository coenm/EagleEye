namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    internal class PhotosCurrentlyProcessing
    {
        [Key]
        public Guid PhotoId { get; set; }

        [Required]
        public DateTimeOffset Start { get; set; }
    }
}