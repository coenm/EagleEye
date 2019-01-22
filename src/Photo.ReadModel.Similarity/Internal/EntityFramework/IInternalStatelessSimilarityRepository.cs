namespace Photo.ReadModel.Similarity.Internal.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    internal interface IInternalStatelessSimilarityRepository
    {
        // Task<Photo> GetByIdAsync(Guid id);
        //
        // Task<Photo> GetByFilenameAsync(Guid id);
        //
        // Task<List<Photo>> GetAllAsync();
        //
        // Task<int> UpdateAsync(Photo item);
        //
        // Task<int> RemoveByIdAsync(params Guid[] itemIds);
        //
        // Task<int> SaveAsync(Photo item);
        [NotNull] HashIdentifiers GetOrAddHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] string identifier);

        Task<HashIdentifiers> GetAddHashIdentifierAsync([NotNull] ISimilarityDbContext db, [NotNull] string messageHashIdentifier, CancellationToken ct = default(CancellationToken));

        Task<List<PhotoHash>> GetPhotoHashesUntilVersionAsync([NotNull] ISimilarityDbContext db, Guid messageId, [NotNull] HashIdentifiers hashIdentifier, int messageVersion, CancellationToken ct = default(CancellationToken));

        Task<PhotoHash> TryGetHashByIdAndHashIdentifierAsync([NotNull] ISimilarityDbContext db, Guid messageId, [NotNull] HashIdentifiers hashIdentifier, CancellationToken ct);

        [NotNull] List<Scores> GetHashScoresByIdAndBeforeVersion([NotNull] ISimilarityDbContext db, int hashIdentifierId, Guid id, int version);

        [NotNull] List<PhotoHash> GetPhotoHashesByHashIdentifier([NotNull] ISimilarityDbContext db, [NotNull] HashIdentifiers hashIdentifier);
    }
}
