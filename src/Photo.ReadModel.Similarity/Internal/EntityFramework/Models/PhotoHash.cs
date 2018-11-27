namespace Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models.Base;

    internal class PhotoHash : VersionedItemBase
    {
        [Required]
        public byte[] Hash { get; set; }

        [Required] // foreign key
        public int HashIdentifiersId { get; set; }

        [Required]
        public HashIdentifiers HashIdentifier { get; set; }
    }
}
