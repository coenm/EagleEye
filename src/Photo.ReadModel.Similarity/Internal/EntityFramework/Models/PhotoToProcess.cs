namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models.Base;

    internal class PhotoToProcess : VersionedItemBase
    {
        [Required]
        public PhotoAction Action { get; set; }

        // foreign key
        [Required]
        public int HashIdentifiersId { get; set; }

        [Required]
        public HashIdentifiers HashIdentifier { get; set; }

        // only required when PhotoAction = Update
        public byte[] Hash { get; set; }
    }
}