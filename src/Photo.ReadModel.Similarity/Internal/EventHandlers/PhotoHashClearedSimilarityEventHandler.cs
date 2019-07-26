namespace EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using Hangfire;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class PhotoHashClearedSimilarityEventHandler : ICancellableEventHandler<PhotoHashCleared>
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly IBackgroundJobClient hangFireClient;

        public PhotoHashClearedSimilarityEventHandler(
            [NotNull] IInternalStatelessSimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory,
            [NotNull] IBackgroundJobClient hangFireClient)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            Guard.Argument(contextFactory, nameof(contextFactory)).NotNull();
            Guard.Argument(hangFireClient, nameof(hangFireClient)).NotNull();
            this.repository = repository;
            this.contextFactory = contextFactory;
            this.hangFireClient = hangFireClient;
        }

        public async Task Handle(PhotoHashCleared message, CancellationToken ct)
        {
            Guard.Argument(message, nameof(message)).NotNull();

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
    }
}
