namespace EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using Hangfire;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class PhotoHashAddedSimilarityEventHandler : ICancellableEventHandler<PhotoHashAdded>
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly IBackgroundJobClient hangFireClient;

        public PhotoHashAddedSimilarityEventHandler(
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

        public async Task Handle(PhotoHashAdded message, CancellationToken ct)
        {
            Guard.Argument(message, nameof(message)).NotNull();

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
