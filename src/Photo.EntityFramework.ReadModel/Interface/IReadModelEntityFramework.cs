namespace Photo.EntityFramework.ReadModel.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Photo.EntityFramework.ReadModel.Interface.Model;

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
