namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    internal class InternalSimilarityRepository : IInternalStatelessSimilarityRepository
    {
        [NotNull]
        [Pure]
        public HashIdentifiers GetOrAddHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] string identifier)
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNullOrWhiteSpace(identifier, nameof(identifier));

            var dbItem = db.HashIdentifiers.FirstOrDefault(x => x.HashIdentifier == identifier);

            if (dbItem != null)
                return dbItem;

            dbItem = new HashIdentifiers
            {
                HashIdentifier = identifier,
            };

            db.HashIdentifiers.Add(dbItem);
            return dbItem;
        }

        [NotNull]
        [Pure]
        public async Task<HashIdentifiers> GetAddHashIdentifierAsync(
            [NotNull] ISimilarityDbContext db,
            [NotNull] string identifier,
            CancellationToken ct = default(CancellationToken))
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNullOrWhiteSpace(identifier, nameof(identifier));

            ct.ThrowIfCancellationRequested();

            var dbItem = await db.HashIdentifiers.FirstOrDefaultAsync(
                    hashIdentifiers => hashIdentifiers.HashIdentifier == identifier, ct)
                .ConfigureAwait(false);

            if (dbItem != null)
                return dbItem;

            dbItem = new HashIdentifiers
            {
                HashIdentifier = identifier,
            };

            await db.HashIdentifiers.AddAsync(dbItem, ct).ConfigureAwait(false);
            return dbItem;
        }

        [CanBeNull]
        [Pure]
        public Task<PhotoHash> TryGetHashByIdAndHashIdentifierAsync(
            [NotNull] ISimilarityDbContext db,
            Guid messageId,
            [NotNull] HashIdentifiers hashIdentifier,
            CancellationToken ct = default(CancellationToken))
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNull(hashIdentifier, nameof(hashIdentifier));

            ct.ThrowIfCancellationRequested();

            return db.PhotoHashes
                .SingleOrDefaultAsync(
                    photoHash =>
                        photoHash.Id == messageId
                        &&
                        photoHash.HashIdentifier == hashIdentifier,
                    ct);
        }

        [Pure]
        [NotNull]
        public List<Scores> GetHashScoresByIdAndBeforeVersion([NotNull] ISimilarityDbContext db, int hashIdentifierId, Guid id, int version)
        {
            DebugGuard.NotNull(db, nameof(db));

            return db.Scores
                .Where(score =>
                    score.HashIdentifierId == hashIdentifierId
                    && (
                        (score.PhotoA == id && score.VersionPhotoA <= version)
                        ||
                        (score.PhotoB == id && score.VersionPhotoB <= version)))
                .ToList();
        }

        [Pure]
        [NotNull]
        public List<PhotoHash> GetPhotoHashesByHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] HashIdentifiers hashIdentifier)
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNull(hashIdentifier, nameof(hashIdentifier));

            return db.PhotoHashes
                .Where(photoHash => photoHash.HashIdentifier == hashIdentifier)
                .ToList();
        }

        [Pure]
        [NotNull]
        public Task<List<PhotoHash>> GetPhotoHashesUntilVersionAsync(
            [NotNull] ISimilarityDbContext db,
            Guid messageId,
            [NotNull] HashIdentifiers hashIdentifier,
            int messageVersion,
            CancellationToken ct = default(CancellationToken))
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNull(hashIdentifier, nameof(hashIdentifier));

            ct.ThrowIfCancellationRequested();

            return db.PhotoHashes
                .Where(photoHash =>
                    photoHash.Id == messageId
                    &&
                    photoHash.HashIdentifier == hashIdentifier
                    &&
                    photoHash.Version <= messageVersion)
                .ToListAsync(ct);
        }

        // public Task<Photo> GetByIdAsync(Guid id)
        // {
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         var result = db.Photos.FirstOrDefault(x => x.Id.Equals(id));
        //         return Task.FromResult(result);
        //     }
        // }
        //
        // public Task<Photo> GetByFilenameAsync(Guid id)
        // {
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         var result = db.Photos.FirstOrDefault(x => x.Id.Equals(id));
        //         return Task.FromResult(result);
        //     }
        // }
        //
        // public Task<List<Photo>> GetAllAsync()
        // {
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         var result = db.Photos.ToList();
        //         return Task.FromResult(result);
        //     }
        // }
        //
        // public async Task<int> UpdateAsync(Photo item)
        // {
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         db.Photos.Update(item);
        //         return await db.SaveChangesAsync().ConfigureAwait(false);
        //     }
        // }
        //
        // public async Task<int> RemoveByIdAsync(params Guid[] itemIds)
        // {
        //     if (itemIds == null || itemIds.Any() == false)
        //         return 0;
        //
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         var items = await db.Photos
        //                             .Where(x => itemIds.Contains(x.Id))
        //                             .ToListAsync()
        //                             .ConfigureAwait(false);
        //
        //         if (items.Any() == false)
        //             return 0;
        //
        //         foreach (var item in items)
        //             db.Photos.Remove(item);
        //
        //         return await db.SaveChangesAsync().ConfigureAwait(false);
        //     }
        // }
        //
        // public async Task<int> SaveAsync(Photo item)
        // {
        //     using (var db = contextFactory.CreateDbContext())
        //     {
        //         await db.Photos.AddAsync(item).ConfigureAwait(false);
        //         return await db.SaveChangesAsync().ConfigureAwait(false);
        //     }
        // }
    }
}
