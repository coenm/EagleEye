namespace Photo.EntityFramework.ReadModel.Internal.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models;

    internal interface IEagleEyeRepository /*: IReadOnlyRepository<MediaItemDb>*/
    {
        Task<Photo> GetByIdAsync(Guid id);

        Task<Photo> GetByFilenameAsync(Guid id);

        Task<List<Photo>> GetAllAsync();

        Task<int> UpdateAsync(Photo item);

        Task<int> RemoveByIdAsync(params Guid[] itemIds);

        Task<int> SaveAsync(Photo item);
    }
}
