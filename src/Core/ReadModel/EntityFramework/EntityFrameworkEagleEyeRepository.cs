namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal class EntityFrameworkEagleEyeRepository : IEagleEyeRepository
    {
        private readonly IEagleEyeDbContextFactory contextFactory;

        public EntityFrameworkEagleEyeRepository(IEagleEyeDbContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public Task<Photo> GetByIdAsync(Guid id)
        {
            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                var result = db.Photos.FirstOrDefault(x => x.Id.Equals(id));
                return Task.FromResult(result);
            }
        }

        public Task<Photo> GetByFilenameAsync(Guid id)
        {
            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                var result = db.Photos.FirstOrDefault(x => x.Id.Equals(id));
                return Task.FromResult(result);
            }
        }

        public Task<List<Photo>> GetAllAsync()
        {
            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                var result = db.Photos.ToList();
                return Task.FromResult(result);
            }
        }

        public async Task<int> UpdateAsync(Photo item)
        {
            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                db.Photos.Update(item);
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> RemoveByIdAsync(params Guid[] itemIds)
        {
            if (itemIds == null || itemIds.Any() == false)
                return 0;

            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                var items = await db.Photos
                                    .Where(x => itemIds.Contains(x.Id))
                                    .ToListAsync()
                                    .ConfigureAwait(false);

                if (items.Any() == false)
                    return 0;

                foreach (var item in items)
                    db.Photos.Remove(item);

                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> SaveAsync(Photo item)
        {
            using (var db = contextFactory.CreateMediaItemDbContext())
            {
                await db.Photos.AddAsync(item).ConfigureAwait(false);
                return await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
