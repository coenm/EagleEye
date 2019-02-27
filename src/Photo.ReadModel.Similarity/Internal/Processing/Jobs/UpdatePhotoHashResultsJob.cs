namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs
{
    using System;
    using System.Linq;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class UpdatePhotoHashResultsJob
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly ISimilarityJobConfiguration configuration;

        public UpdatePhotoHashResultsJob(
            [NotNull] IInternalStatelessSimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory,
            [NotNull] ISimilarityJobConfiguration configuration)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            Guard.NotNull(configuration, nameof(configuration));
            this.repository = repository;
            this.contextFactory = contextFactory;
            this.configuration = configuration;
        }

        public void Execute(Guid photoId, int version, string hashIdentifierString)
        {
            DebugGuard.NotNullOrWhiteSpace(hashIdentifierString, nameof(hashIdentifierString));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = repository.GetHashIdentifier(db, hashIdentifierString);
                if (hashIdentifier == null)
                    return;

                var currentItem = repository.GetPhotoHashByIdAndHashIdentifier(db, photoId, hashIdentifier);
                if (currentItem == null)
                    return;

                var outdatedScores = repository.GetOutdatedScores(db, photoId, hashIdentifier, version);
                if (outdatedScores.Any())
                    repository.DeleteScores(db, outdatedScores);

                var allPhotoHashes = repository
                    .GetPhotoHashesByHashIdentifier(db, hashIdentifier)
                    .Where(item => item.Id != photoId)
                    .ToList();

                var currentPhotoHashValue = currentItem.Hash;

                foreach (var item in allPhotoHashes)
                {
                    var hashUnsignedLongPhotoB = item.Hash;

                    var value = CoenM.ImageHash.CompareHash.Similarity(currentPhotoHashValue, hashUnsignedLongPhotoB);

                    if (value < configuration.ThresholdPercentageSimilarityStorage)
                        continue;

                    Scores score;
                    if (photoId.CompareTo(item.Id) < 0)
                    {
                        score = new Scores
                        {
                            VersionPhotoA = version,
                            VersionPhotoB = item.Version,
                            PhotoA = photoId,
                            PhotoB = item.Id,
                            HashIdentifier = hashIdentifier,
                            Score = value,
                        };
                    }
                    else
                    {
                        score = new Scores
                        {
                            VersionPhotoB = version,
                            VersionPhotoA = item.Version,
                            PhotoB = photoId,
                            PhotoA = item.Id,
                            HashIdentifier = hashIdentifier,
                            Score = value,
                        };
                    }

                    db.Scores.Add(score);
                }

                // if saving goes wrong, then this Job will fail and re-scheduled.
                db.SaveChanges();
            }
        }
    }
}
