namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    internal interface ISimilarityDbContext : IDisposable
    {
        DbSet<HashIdentifiers> HashIdentifiers { get; }

        DbSet<PhotoHash> PhotoHashes { get; }

        DbSet<Scores> Scores { get; }

        Task SaveChangesAsync(CancellationToken ct = default(CancellationToken));

        void SaveChanges();
    }
}
