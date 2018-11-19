namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Models;

    public interface IEagleEyeRepository /*: IReadOnlyRepository<MediaItemDb>*/
    {
        Task<Photo> GetByIdAsync(Guid id);

        Task<Photo> GetByFilenameAsync(Guid id);

        Task<IEnumerable<Photo>> GetAllAsync();

        Task<int> UpdateAsync(Photo item);

        Task<int> RemoveByIdAsync(params Guid[] itemIds);

        Task<int> SaveAsync(Photo item);
    }
}
