namespace EagleEye.Photo.ReadModel.EntityFramework.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.EntityFramework.Interface.Model;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IReadModelEntityFramework
    {
        [ItemCanBeNull]
        Task<IEnumerable<Photo>> GetAllPhotosAsync();

        [CanBeNull]
        [ItemCanBeNull]
        Task<Photo> GetPhotoByGuidAsync(Guid id);
    }
}
