namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkMediaItemRepository : IMediaItemRepository
    {
        private readonly IMediaItemDbContextFactory _contextFactory;

        public EntityFrameworkMediaItemRepository(IMediaItemDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Task<MediaItemDb> GetByIdAsync(Guid id)
        {
            using (var db = _contextFactory.CreateMediaItemDbContext())
            {
                var result = db.MediaItems.FirstOrDefault(x => x.Id.Equals(id));
                return Task.FromResult(result);
            }
        }

        public Task<MediaItemDb> GetByFilenameAsync(Guid id)
        {
            using (var db = _contextFactory.CreateMediaItemDbContext())
            {
                var result = db.MediaItems.FirstOrDefault(x => x.Id.Equals(id));
                return Task.FromResult(result);
            }
        }

        public async Task<int> UpdateAsync(MediaItemDb item)
        {
            using (var db = _contextFactory.CreateMediaItemDbContext())
            {
                db.MediaItems.Update(item);
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> RemoveByIdAsync(params Guid[] itemIds)
        {
            if (itemIds == null || itemIds.Any() == false)
                return 0;

            using (var db = _contextFactory.CreateMediaItemDbContext())
            {
                var items = await db.MediaItems
                                    .Where(x => itemIds.Contains(x.Id))
                                    .ToListAsync()
                                    .ConfigureAwait(false);

                if (items.Any() == false)
                    return 0;

                foreach (var item in items)
                    db.MediaItems.Remove(item);

                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> SaveAsync(MediaItemDb item)
        {
            using (var db = _contextFactory.CreateMediaItemDbContext())
            {
                await db.MediaItems.AddAsync(item).ConfigureAwait(false);
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}