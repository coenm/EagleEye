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

        public DbSet<Scores> Scores { get; set; }

        public Task EnsureCreatedAsync() => Database.EnsureCreatedAsync();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PhotoHash>()
                .HasKey(table => new { table.Id, table.HashIdentifiersId });

            modelBuilder.Entity<HashIdentifiers>()
                .HasIndex(table => table.HashIdentifier)
                .IsUnique();

            modelBuilder.Entity<Scores>()
                .HasKey(table => new { table.PhotoA, table.PhotoB, table.HashIdentifierId });
        }
    }
}
