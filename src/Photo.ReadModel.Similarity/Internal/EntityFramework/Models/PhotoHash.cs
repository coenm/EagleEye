namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models
{
    using System.ComponentModel.DataAnnotations;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models.Base;

    internal class PhotoHash : VersionedItemBase
    {
        [Required]
        public ulong Hash { get; set; }

        [Required] // foreign key
        public int HashIdentifiersId { get; set; }

        [Required]
        public HashIdentifiers HashIdentifier { get; set; }
    }
}
