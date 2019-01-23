namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal class SimilarityDbContext : DbContext, ISimilarityDbContext
    {
        public SimilarityDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<HashIdentifiers> HashIdentifiers { get; set; }

        public DbSet<PhotoHash> PhotoHashes { get; set; }

        public DbSet<Scores> Scores { get; set; }

        Task ISimilarityDbContext.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);

        void ISimilarityDbContext.SaveChanges() => SaveChanges();

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
