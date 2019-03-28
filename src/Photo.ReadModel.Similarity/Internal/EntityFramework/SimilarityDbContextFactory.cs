namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using Helpers.Guards; using Dawn;
    using Microsoft.EntityFrameworkCore;

    internal class SimilarityDbContextFactory : ISimilarityDbContextFactory
    {
        private readonly DbContextOptions<SimilarityDbContext> options;

        public SimilarityDbContextFactory(DbContextOptions<SimilarityDbContext> options)
        {
            Dawn.Guard.Argument(options, nameof(options)).NotNull();
            this.options = options;
        }

        public async Task Initialize()
        {
            using (var db = CreateDbContext())
            {
                // await db.Database.OpenConnectionAsync();
                await db.Database.EnsureCreatedAsync();
            }
        }

        ISimilarityDbContext ISimilarityDbContextFactory.CreateDbContext() => CreateDbContext();

        public SimilarityDbContext CreateDbContext() => new SimilarityDbContext(options);
    }
}
