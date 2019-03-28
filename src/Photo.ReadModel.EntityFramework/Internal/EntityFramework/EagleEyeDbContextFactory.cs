﻿namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using Helpers.Guards; using Dawn;
    using Microsoft.EntityFrameworkCore;

    internal class EagleEyeDbContextFactory : IEagleEyeDbContextFactory
    {
        private readonly DbContextOptions<EagleEyeDbContext> options;

        public EagleEyeDbContextFactory(DbContextOptions<EagleEyeDbContext> options)
        {
            Helpers.Guards.Guard.NotNull(options, nameof(options));
            this.options = options;
        }

        public async Task Initialize()
        {
            using (var db = CreateMediaItemDbContext())
            {
                // await db.Database.OpenConnectionAsync();
                await db.Database.EnsureCreatedAsync();
            }
        }

        public EagleEyeDbContext CreateMediaItemDbContext() => new EagleEyeDbContext(options);
    }
}
