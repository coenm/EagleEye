namespace Photo.ReadModel.EntityFramework.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Photo.ReadModel.EntityFramework.Interface.Model;

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
