namespace EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using Hangfire;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class SimilarityEventHandlers :
        ICancellableEventHandler<PhotoHashCleared>,
        ICancellableEventHandler<PhotoHashUpdated>
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly IBackgroundJobClient hangFireClient;

        public SimilarityEventHandlers(
            [NotNull] IInternalStatelessSimilarityRepository repository,
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
            Guard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await repository.GetAddHashIdentifierAsync(db, message.HashIdentifier, ct)
                    .ConfigureAwait(false);

                var itemToRemove = await repository.GetPhotoHashesUntilVersionAsync(db, message.Id, hashIdentifier, message.Version, ct)
                    .ConfigureAwait(false);

                if (itemToRemove.Any())
                    db.PhotoHashes.RemoveRange(itemToRemove);

                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            hangFireClient.Enqueue<ClearPhotoHashResultsJob>(job => job.Execute(message.Id, message.Version, message.HashIdentifier));
        }

        public async Task Handle(PhotoHashUpdated message, CancellationToken ct)
        {
            DebugGuard.NotNull(message, nameof(message));

            using (var db = contextFactory.CreateDbContext())
            {
                var hashIdentifier = await repository.GetAddHashIdentifierAsync(db, message.HashIdentifier, ct)
                    .ConfigureAwait(false);

                var existingItem = await repository.TryGetPhotoHashByIdAndHashIdentifierAsync(db, message.Id, hashIdentifier, ct)
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
                    await db.PhotoHashes.AddAsync(newItem, ct)
                        .ConfigureAwait(false);
                }

                await db.SaveChangesAsync(ct)
                    .ConfigureAwait(false);
            }

            hangFireClient.Enqueue<UpdatePhotoHashResultsJob>(job => job.Execute(message.Id, message.Version, message.HashIdentifier));
        }
    }
}
