namespace EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;

    internal class InternalSimilarityRepository : IInternalStatelessSimilarityRepository
    {
        [CanBeNull]
        [Pure]
        public HashIdentifiers GetHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] string identifier)
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(identifier, nameof(identifier));

            return db.HashIdentifiers.SingleOrDefault(item => item.HashIdentifier == identifier);
        }

        [NotNull]
        [Pure]
        public HashIdentifiers GetOrAddHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] string identifier)
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(identifier, nameof(identifier));

            var dbItem = GetHashIdentifier(db, identifier);

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
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNullOrWhiteSpace(identifier, nameof(identifier));

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
        public PhotoHash GetPhotoHashByIdAndHashIdentifier([NotNull] ISimilarityDbContext db, Guid photoHashId, [NotNull] HashIdentifiers hashIdentifier)
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(hashIdentifier, nameof(hashIdentifier));

            return db.PhotoHashes.SingleOrDefault(
                photoHash =>
                    photoHash.Id == photoHashId
                    &&
                    photoHash.HashIdentifier == hashIdentifier);
        }

        [CanBeNull]
        [Pure]
        public Task<PhotoHash> TryGetPhotoHashByIdAndHashIdentifierAsync(
            [NotNull] ISimilarityDbContext db,
            Guid messageId,
            [NotNull] HashIdentifiers hashIdentifier,
            CancellationToken ct = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(hashIdentifier, nameof(hashIdentifier));

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
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));

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
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(hashIdentifier, nameof(hashIdentifier));

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
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(hashIdentifier, nameof(hashIdentifier));

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

        [NotNull]
        [Pure]
        public List<Scores> GetOutdatedScores([NotNull] ISimilarityDbContext db, Guid photoId, [NotNull] HashIdentifiers hashIdentifier, int version)
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(hashIdentifier, nameof(hashIdentifier));

            return db.Scores
                .Where(score =>
                    score.HashIdentifier == hashIdentifier
                    && (
                        (score.PhotoA == photoId && score.VersionPhotoA <= version)
                        ||
                        (score.PhotoB == photoId && score.VersionPhotoB <= version)))
                .ToList();
        }

        public void DeleteScores([NotNull] ISimilarityDbContext db, [NotNull] IEnumerable<Scores> scores)
        {
            DebugHelpers.Guards.Guard.NotNull(db, nameof(db));
            DebugHelpers.Guards.Guard.NotNull(scores, nameof(scores));

            if (scores.Any())
                db.Scores.RemoveRange(scores);
        }
    }
}
