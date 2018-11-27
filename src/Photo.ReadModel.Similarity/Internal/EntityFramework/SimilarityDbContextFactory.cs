namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using Helpers.Guards;

    using Microsoft.EntityFrameworkCore;

    internal class SimilarityDbContextFactory : ISimilarityDbContextFactory
    {
        private readonly DbContextOptions<SimilarityDbContext> options;

        public SimilarityDbContextFactory(DbContextOptions<SimilarityDbContext> options)
        {
            Guard.NotNull(options, nameof(options));
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

        public SimilarityDbContext CreateDbContext() => new SimilarityDbContext(options);
    }
}
