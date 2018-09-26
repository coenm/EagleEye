namespace EagleEye.Core.ReadModel.EntityFramework.Dto
{
    using System.Collections.Generic;

    public class MediaItemDto
    {
        public List<string> Tags { get; set; }

        public List<string> Persons { get; set; }

        public LocationDto Location { get; set; }
    }
}