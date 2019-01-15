namespace Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using Hangfire;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Photo.ReadModel.Similarity.Internal.Processing;

    [UsedImplicitly]
    internal class SimilarityEventHandlers :
        ICancellableEventHandler<PhotoHashCleared>,
        ICancellableEventHandler<PhotoHashUpdated>
    {
        [NotNull] private readonly ISimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly IBackgroundJobClient hangFireClient;

        public SimilarityEventHandlers(
            [NotNull] ISimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory,
            [NotNull] IBackgroundJobClient hangFireClient)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            Guard.NotNull(hangFireClient, nameof(hangFireClient));
            this.repository = repository;
            this.contextFactory = contextFactory;
            this.hangFireClient = hangFireClient;
        }

        public async Task Handle(PhotoHashCleared message, CancellationToken ct)
        {
            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await GetAddHashIdentifierAsync(db, message.HashIdentifier, ct)
                    .ConfigureAwait(false);

                var itemToRemove = await db.PhotoHashes
                    .Where(x =>
                        x.Id == message.Id
                        &&
                        x.HashIdentifier == hashIdentifier
                        &&
                        x.Version <= message.Version)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                if (itemToRemove.Any())
                {
                    db.PhotoHashes.RemoveRange(itemToRemove);
                }

                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            hangFireClient.Enqueue<ClearPhotoHashResultsJob>(job => job.Execute(message.Id, message.Version, message.HashIdentifier));
        }

        public async Task Handle(PhotoHashUpdated message, CancellationToken ct)
        {
            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await GetAddHashIdentifierAsync(db, message.HashIdentifier, ct).ConfigureAwait(false);

                var existingItem = await db.PhotoHashes
                                           .SingleOrDefaultAsync(x => x.Id == message.Id && x.HashIdentifier == hashIdentifier, ct)
                                           .ConfigureAwait(false);

                if (existingItem != null)
                {
                    if (existingItem.Version > message.Version)
                    {
                        return;
                    }

                    existingItem.Version = message.Version;
                    existingItem.Hash = message.Hash;

                    db.PhotoHashes.Update(existingItem);
                }
                else
                {
                    var newItem = new PhotoHash
                                  {
                                      Id = message.Id,
                                      Version = message.Version,
                                      HashIdentifier = hashIdentifier,
                                      Hash = message.Hash,
                                  };
                    await db.PhotoHashes.AddAsync(newItem, ct);
                }

                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            hangFireClient.Enqueue<UpdatePhotoHashResultsJob>(job => job.Execute(message.Id, message.Version, message.HashIdentifier));
        }

        private static async Task<HashIdentifiers> GetAddHashIdentifierAsync(
            [NotNull] SimilarityDbContext db,
            [NotNull] string identifier,
            CancellationToken ct = default(CancellationToken))
        {
            DebugGuard.NotNull(db, nameof(db));
            DebugGuard.NotNullOrWhiteSpace(identifier, nameof(identifier));

            ct.ThrowIfCancellationRequested();

            var dbItem = await db.HashIdentifiers.FirstOrDefaultAsync(x => x.HashIdentifier == identifier, ct);

            if (dbItem != null)
                return dbItem;

            dbItem = new HashIdentifiers
                     {
                         HashIdentifier = identifier,
                     };

            await db.HashIdentifiers.AddAsync(dbItem, ct).ConfigureAwait(false);
            return dbItem;
        }
    }
}
