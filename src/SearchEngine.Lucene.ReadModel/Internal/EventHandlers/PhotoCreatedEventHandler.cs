namespace SearchEngine.LuceneNet.ReadModel.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Core.Domain.Events;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;
    using SearchEngine.LuceneNet.ReadModel.Internal.LuceneNet;
    using SearchEngine.LuceneNet.ReadModel.Internal.Model;

    internal class PhotoCreatedEventHandler :
            ICancellableEventHandler<PhotoCreated>,
            ICancellableEventHandler<TagsAddedToPhoto>,
            ICancellableEventHandler<TagsRemovedFromPhoto>,
            ICancellableEventHandler<PersonsAddedToPhoto>,
            ICancellableEventHandler<PersonsRemovedFromPhoto>,
            ICancellableEventHandler<LocationClearedFromPhoto>,
            ICancellableEventHandler<LocationSetToPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly PhotoIndex photoIndex;

        public PhotoCreatedEventHandler([NotNull] PhotoIndex photoIndex)
        {
            Guard.NotNull(photoIndex, nameof(photoIndex));
            this.photoIndex = photoIndex;
        }

        public Task Handle([NotNull] PhotoCreated message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var storedItem = photoIndex.Search(message.Id);

            // should be null
            // otherwise,delete it
            if (storedItem != null)
            {
                photoIndex.DeleteById(message.Id);
            }

            // not interested in message.FileHash.

            var photo = new Photo
            {
                Id = message.Id,
                Version = message.Version,
                FileName = message.FileName,
                FileMimeType = message.MimeType,
                Tags = message.Tags?.ToList(),
                Persons = message.Persons?.ToList(),
                // TimeStamp
            };

            return photoIndex.IndexMediaFileAsync(photo);
        }

        public async Task Handle(TagsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(PersonsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));
            DebugGuard.NotNull(message.Location, $"{nameof(message)}.{nameof(message.Location)}");

            await Task.Delay(0, token).ConfigureAwait(false);
        }
    }
}
