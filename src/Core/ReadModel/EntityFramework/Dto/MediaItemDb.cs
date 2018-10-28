namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    using System.ComponentModel.DataAnnotations;

    public class MediaItemDb : VersionedDb
    {
        [Required]
        [MaxLength(256)] // dummy, but demonstrates possibilities
        public string Filename { get; set; }

        // for now, the easiest solution is to serialize the data and store it as a string.
        // why not? ;-)
        [Required]
        public string SerializedMediaItemDto { get; set; }
    }
}
