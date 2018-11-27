namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;

    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    internal class SimilarityDbContext : DbContext
    {
        public SimilarityDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<HashIdentifiers> HashIdentifiers { get; set; }

        public DbSet<PhotoHash> PhotoHashes { get; set; }

        public Task EnsureCreatedAsync() => Database.EnsureCreatedAsync();
    }
}
