namespace Photo.ReadModel.Similarity.Internal.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;

    [UsedImplicitly]
    internal class SimilarityEventHandlers :
        ICancellableEventHandler<PhotoCreated>,
        ICancellableEventHandler<FileHashUpdated>,
        ICancellableEventHandler<PhotoHashCleared>,
        ICancellableEventHandler<PhotoHashUpdated>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly ISimilarityRepository repository;

        public SimilarityEventHandlers([NotNull] ISimilarityRepository repository)
        {
            Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
        }

        public async Task Handle(PhotoCreated message, CancellationToken token)
        {
            DebugGuard.NotNull(message, nameof(message));
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
            // todo fill in
            await Task.Delay(9).ConfigureAwait(false);
        }

        public async Task Handle(PhotoHashUpdated message, CancellationToken token)
        {
            DebugGuard.NotNull(message, nameof(message));
            // todo fill in
            await Task.Delay(9).ConfigureAwait(false);
        }
    }
}
