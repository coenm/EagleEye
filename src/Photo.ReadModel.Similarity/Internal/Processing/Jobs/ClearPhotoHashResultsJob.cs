﻿namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs
{
    using System;
    using System.Linq;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ClearPhotoHashResultsJob
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;

        public ClearPhotoHashResultsJob(
            [NotNull] IInternalStatelessSimilarityRepository repository,
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
                var hashIdentifier = repository.GetOrAddHashIdentifier(db, hashIdentifierString);

                var itemsToDelete = repository.GetHashScoresByIdAndBeforeVersion(db, hashIdentifier.Id, id, version);

                if (itemsToDelete.Any())
                    db.Scores.RemoveRange(itemsToDelete);

                db.SaveChanges();
            }
        }
    }
}