namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;

    using Microsoft.EntityFrameworkCore;

    public class MediaItemDbContextFactory : IMediaItemDbContextFactory
    {
        private readonly DbContextOptions<MediaItemDbContext> _options;

        public MediaItemDbContextFactory(DbContextOptions<MediaItemDbContext> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public MediaItemDbContext CreateMediaItemDbContext() => new MediaItemDbContext(_options);
    }
}