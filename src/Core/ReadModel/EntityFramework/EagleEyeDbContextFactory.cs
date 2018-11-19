namespace EagleEye.Core.ReadModel.EntityFramework
{
    using Helpers.Guards;

    using Microsoft.EntityFrameworkCore;

    public class EagleEyeDbContextFactory : IEagleEyeDbContextFactory
    {
        private readonly DbContextOptions<EagleEyeDbContext> options;

        public EagleEyeDbContextFactory(DbContextOptions<EagleEyeDbContext> options)
        {
            Guard.NotNull(options, nameof(options));
            this.options = options;
        }

        public EagleEyeDbContext CreateMediaItemDbContext() => new EagleEyeDbContext(options);
    }
}
