namespace Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using NLog;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;

    [UsedImplicitly]
    internal class SimilarityEventHandlers :
        ICancellableEventHandler<PhotoCreated>,
        ICancellableEventHandler<FileHashUpdated>,
        ICancellableEventHandler<PhotoHashCleared>,
        ICancellableEventHandler<PhotoHashUpdated>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly ISimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;

        public SimilarityEventHandlers([NotNull] ISimilarityRepository repository, [NotNull] ISimilarityDbContextFactory contextFactory)
        {
            Guard.NotNull(repository, nameof(repository));
            Guard.NotNull(contextFactory, nameof(contextFactory));
            this.repository = repository;
            this.contextFactory = contextFactory;
        }

        public async Task Handle(PhotoCreated message, CancellationToken token)
        {
            DebugGuard.NotNull(message, nameof(message));

            // add hash identifier if not exist.



            // todo fill in
            await Task.Delay(9).ConfigureAwait(false);
        }

        public Task Handle(FileHashUpdated message, CancellationToken token)
        {
            // do nothing with this at the moment...
            return Task.CompletedTask;
        }

        public async Task Handle(PhotoHashCleared message, CancellationToken token)
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(9).ConfigureAwait(false);
        }

        public async Task Handle(PhotoHashUpdated message, CancellationToken token)
        {
            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var dbItem = await db.HashIdentifiers.FirstOrDefaultAsync(x => x.HashIdentifier == message.HashIdentifier, cancellationToken: token);

                if (dbItem == null)
                {
                    dbItem = new HashIdentifiers
                    {
                        HashIdentifier = message.HashIdentifier,
                    };
                    await db.HashIdentifiers.AddAsync(dbItem, token).ConfigureAwait(false);
                }

                var dbItem2 = await db.PhotoHashes.SingleOrDefaultAsync(x => x.Id == message.Id, cancellationToken: token);
                if (dbItem2 == null)
                {
                    // create
                    await db.PhotoHashes.AddAsync(
                        new PhotoHash
                        {
                            Id = message.Id,
                            HashIdentifier = dbItem,
                            Hash = message.Hash,
                            Version = message.Version,
                            HashIdentifiersId = dbItem.Id,
                        },
                        token).ConfigureAwait(false);
                }
                else
                {
                    dbItem2.Version = message.Version;
                    dbItem2.Hash = message.Hash;
                    dbItem2.HashIdentifiersId = dbItem.Id;
                    dbItem2.HashIdentifier = dbItem;
                    db.PhotoHashes.Update(dbItem2);
                }

                await db.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}
