namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal interface ISimilarityDbContext : IDisposable
    {
        DbSet<HashIdentifiers> HashIdentifiers { get; }

        DbSet<PhotoHash> PhotoHashes { get; }

        DbSet<Scores> Scores { get; }

        Task SaveChangesAsync(CancellationToken ct = default);

        void SaveChanges();
    }
}
