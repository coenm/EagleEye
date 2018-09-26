namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    public interface IMediaItemRepository /*: IReadOnlyRepository<MediaItemDb>*/
    {
        Task<MediaItemDb> GetByIdAsync(Guid id);

        Task<MediaItemDb> GetByFilenameAsync(Guid id);

        Task<IEnumerable<MediaItemDb>> GetAllAsync();

        Task<int> UpdateAsync(MediaItemDb item);

        Task<int> RemoveByIdAsync(params Guid[] itemIds);

        Task<int> SaveAsync(MediaItemDb item);
    }
}