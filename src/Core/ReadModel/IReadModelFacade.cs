namespace EagleEye.Core.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IReadModelFacade
    {
        [ItemCanBeNull]
        Task<IEnumerable<Model.Photo>> GetAllPhotosAsync();

        [CanBeNull]
        [ItemCanBeNull]
        Task<Model.Photo> GetPhotoByGuidAsync(Guid id);
    }
}
