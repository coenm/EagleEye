namespace Photo.ReadModel.Similarity.Internal.Processing
{
    using System;
    using System.Linq;

    using Hangfire;

    using Helpers.Guards;
    using JetBrains.Annotations;

    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    [UsedImplicitly]
    internal class UpdatePhotoHashResultsJob
    {
        [NotNull] private readonly ISimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;

        public UpdatePhotoHashResultsJob([NotNull] ISimilarityRepository repository, [NotNull] ISimilarityDbContextFactory contextFactory)
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

                var currentItem = db.PhotoHashes.SingleOrDefault(x => x.HashIdentifier == hashIdentifier && id == x.Id);

                if (currentItem == null)
                    return;



                var xx = db.Scores
                           .Where(x => x.HashIdentifierId == hashIdentifier.Id && x.PhotoA == id && x.VersionPhotoA <= version)
                           .ToList();

                // If the system architecture is little-endian (that is, little end first),
                // reverse the byte array.
                // if (BitConverter.IsLittleEndian)
                //     Array.Reverse(bytes);
                var hashUnsignedLong = BitConverter.ToUInt64(currentItem.Hash, 0);

                foreach (var score in xx)
                {
                    // update existing.

                    // If the system architecture is little-endian (that is, little end first),
                    // reverse the byte array.
                    // if (BitConverter.IsLittleEndian)
                    //     Array.Reverse(bytes);

                    var photoB = db.PhotoHashes.SingleOrDefault(x => x.HashIdentifier == hashIdentifier && x.Id == score.PhotoB);
                    if (photoB == null)
                    {
                        db.Scores.Remove(score);
                        continue;
                    }

                    var hashUnsignedLongPhotoB = BitConverter.ToUInt64(photoB.Hash, 0);
                    // Console.WriteLine("int: {0}", i);
                    //
                    // byte[] b = BitConverter.GetBytes(i);

                    var value = CoenM.ImageHash.CompareHash.Similarity(hashUnsignedLong, hashUnsignedLongPhotoB);

                    if (value < 80)
                    {
                        // remove
                        db.Scores.Remove(score);
                    }
                    else
                    {
                        // update
                        score.VersionPhotoA = version;
                        score.VersionPhotoB = version;
                        score.Score = value;
                        db.Scores.Update(score);
                    }
                }


                var itemsToDelete = db.Scores
                                      .Where(x =>
                                                 x.HashIdentifierId == hashIdentifier.Id
                                                 && (
                                                        (x.PhotoA == id && x.VersionPhotoA <= version)
                                                        ||
                                                        (x.PhotoB == id && x.VersionPhotoB <= version)))
                                      .ToList();

                if (itemsToDelete.Any())
                {
                    db.Scores.RemoveRange(itemsToDelete);
                }


                var allHashes = db.PhotoHashes
                                  .Where(x => x.HashIdentifier == hashIdentifier && x.Id != id)
                                  .ToList();

                foreach (var item in allHashes)
                {
                    var hashUnsignedLongPhotoB = BitConverter.ToUInt64(item.Hash, 0);

                    // Console.WriteLine("int: {0}", i);
                    //
                    // byte[] b = BitConverter.GetBytes(i);

                    var value = CoenM.ImageHash.CompareHash.Similarity(hashUnsignedLong, hashUnsignedLongPhotoB);

                    if (value >= 80)
                    {
                        // update
                        var score = new Scores
                                    {
                                        VersionPhotoA = version,
                                        VersionPhotoB = item.Version,
                                        PhotoA = id,
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
