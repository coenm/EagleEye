namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;

    internal interface IEagleEyeRepository /*: IReadOnlyRepository<MediaItemDb>*/
    {
        Task<Photo> GetByIdAsync(Guid id);

        Task<List<Photo>> GetAllAsync();

        Task<int> UpdateAsync(Photo item);

        Task<int> RemoveByIdAsync(params Guid[] itemIds);

        Task<int> SaveAsync(Photo item);
    }
}
