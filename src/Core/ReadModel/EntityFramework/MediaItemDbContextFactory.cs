namespace EagleEye.Core.ReadModel.EntityFramework
{
    using System;

    using Microsoft.EntityFrameworkCore;

    public class MediaItemDbContextFactory : IMediaItemDbContextFactory
    {
        private readonly DbContextOptions<MediaItemDbContext> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaItemDbContextFactory"/> class.
        /// </summary>
        public MediaItemDbContextFactory(DbContextOptions<MediaItemDbContext> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public MediaItemDbContext CreateMediaItemDbContext() => new MediaItemDbContext(_options);
    }
}