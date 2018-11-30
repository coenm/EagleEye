namespace Photo.ReadModel.Similarity.Internal.EventHandlers
{
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

    [UsedImplicitly]
    internal class SimilarityEventHandlers :
        ICancellableEventHandler<PhotoHashCleared>,
        ICancellableEventHandler<PhotoHashUpdated>
    {
        [NotNull] private readonly ISimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;

        public SimilarityEventHandlers([NotNull] ISimilarityRepository repository, [NotNull] ISimilarityDbContextFactory contextFactory)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            this.repository = repository;
            this.contextFactory = contextFactory;
        }

        public async Task Handle(PhotoHashCleared message, CancellationToken ct)
        {
            BackgroundJob.Enqueue(() => null);

            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await GetAddHashIdentifierAsync(db, message.HashIdentifier, ct);

                await db.ToProcess.AddAsync(
                                            new PhotoToProcess
                                            {
                                                Action = PhotoAction.Delete,
                                                HashIdentifier = hashIdentifier,
                                                HashIdentifiersId = hashIdentifier.Id,
                                                Id = message.Id,
                                                Hash = null,
                                                Version = message.Version,
                                            },
                                            ct);

                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
        }

        public async Task Handle(PhotoHashUpdated message, CancellationToken ct)
        {
            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await GetAddHashIdentifierAsync(db, message.HashIdentifier, ct);

                await db.ToProcess.AddAsync(
                                            new PhotoToProcess
                                            {
                                                Action = PhotoAction.Update,
                                                HashIdentifier = hashIdentifier,
                                                HashIdentifiersId = hashIdentifier.Id,
                                                Id = message.Id,
                                                Hash = message.Hash,
                                                Version = message.Version,
                                            },
                                            ct);

                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }
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
