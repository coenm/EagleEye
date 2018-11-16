namespace EagleEye.Core.ReadModel.EntityFramework
{
    using Helpers.Guards;

    using Microsoft.EntityFrameworkCore;

    public class MediaItemDbContextFactory : IMediaItemDbContextFactory
    {
        private readonly DbContextOptions<MediaItemDbContext> options;

        public MediaItemDbContextFactory(DbContextOptions<MediaItemDbContext> options)
        {
            Guard.NotNull(options, nameof(options));
            this.options = options;
        }

        public MediaItemDbContext CreateMediaItemDbContext() => new MediaItemDbContext(options);
    }
}
