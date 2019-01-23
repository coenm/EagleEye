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

        public UpdatePhotoHashResultsJob(
            [NotNull] IInternalStatelessSimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            this.repository = repository;
            this.contextFactory = contextFactory;
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

                var currentPhotoHashValue = GetUnsignedLongHashValue(currentItem.Hash);

                foreach (var item in allPhotoHashes)
                {
                    var hashUnsignedLongPhotoB = GetUnsignedLongHashValue(item.Hash);

                    // byte[] b = BitConverter.GetBytes(i);

                    var value = CoenM.ImageHash.CompareHash.Similarity(currentPhotoHashValue, hashUnsignedLongPhotoB);

                    if (value >= 80)
                    {
                        // todo photoA, photoB ordered.
                        var score = new Scores
                        {
                            VersionPhotoA = version,
                            VersionPhotoB = item.Version,
                            PhotoA = photoId,
                            PhotoB = item.Id,
                            HashIdentifier = hashIdentifier,
                            Score = value,
                        };

                        db.Scores.Add(score);
                    }
                }

                db.SaveChanges();
            }
        }

        private ulong GetUnsignedLongHashValue([NotNull] byte[] hashValue)
        {
            DebugGuard.NotNull(hashValue, nameof(hashValue));
            DebugGuard.MustBeEqualTo(hashValue.Length, 8, $"{nameof(hashValue)}.{nameof(hashValue.Length)}");

            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            // if (BitConverter.IsLittleEndian)
            //     Array.Reverse(bytes);

            return BitConverter.ToUInt64(hashValue, 0);
        }
    }
}
