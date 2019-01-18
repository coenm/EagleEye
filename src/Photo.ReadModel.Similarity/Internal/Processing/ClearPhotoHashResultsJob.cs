namespace Photo.ReadModel.Similarity.Internal.Processing
{
    using System;
    using System.Linq;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    [UsedImplicitly]
    internal class ClearPhotoHashResultsJob
    {
        [NotNull] private readonly ISimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;

        public ClearPhotoHashResultsJob(
            [NotNull] ISimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            this.repository = repository;
            this.contextFactory = contextFactory;
        }

        public void Execute(Guid id, int version, string hashIdentifierString)
        {
            DebugGuard.NotNullOrWhiteSpace(hashIdentifierString, nameof(hashIdentifierString));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = GetAddHashIdentifier(db, hashIdentifierString);

                var itemsToDelete = db.Scores
                    .Where(x =>
                        x.HashIdentifierId == hashIdentifier.Id
                        && (
                            (x.PhotoA == id && x.VersionPhotoA <= version)
                            ||
                            (x.PhotoB == id && x.VersionPhotoB <= version)))
                    .ToList();

                if (itemsToDelete.Any())
                    db.Scores.RemoveRange(itemsToDelete);

                db.SaveChanges();
            }
        }

        private static HashIdentifiers GetAddHashIdentifier(
            [NotNull] SimilarityDbContext db,
            [NotNull] string identifier)
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
    }
}
