namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using NLog;

    internal class PhotoCreatedEventHandler :
            ICancellableEventHandler<PhotoCreated>,
            ICancellableEventHandler<TagsAddedToPhoto>,
            ICancellableEventHandler<TagsRemovedFromPhoto>,
            ICancellableEventHandler<PersonsRemovedFromPhoto>,
            ICancellableEventHandler<LocationClearedFromPhoto>,
            ICancellableEventHandler<LocationSetToPhoto>,
            ICancellableEventHandler<DateTimeTakenChanged>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IPhotoIndex photoIndex;

        public PhotoCreatedEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Helpers.Guards.Guard.NotNull(photoIndex, nameof(photoIndex));
            this.photoIndex = photoIndex;
        }

        public Task Handle([NotNull] PhotoCreated message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            var photo = new Photo
            {
                Id = message.Id,
                Version = message.Version,
                FileName = message.FileName,
                FileMimeType = message.MimeType,
                DateTimeTaken = null, // todo
            };

            return photoIndex.ReIndexMediaFileAsync(photo);
        }

        public async Task Handle(TagsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));
            DebugHelpers.Guards.Guard.NotNull(message.Location, $"{nameof(message)}.{nameof(message.Location)}");

            await Task.Delay(0, token).ConfigureAwait(false);
        }

        public Task Handle(DateTimeTakenChanged message, CancellationToken token = new CancellationToken())
        {
            DebugHelpers.Guards.Guard.NotNull(message, nameof(message));

            var storedItem = photoIndex.Search(message.Id);

            if (storedItem == null)
                throw new NullReferenceException();

            storedItem.DateTimeTaken = new Timestamp(message.DateTimeTaken, Convert(message.Precision));

            return photoIndex.ReIndexMediaFileAsync(storedItem);
        }

        private TimestampPrecision Convert(EagleEye.Photo.Domain.Aggregates.TimestampPrecision input)
        {
            switch (input)
            {
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Year:
                return TimestampPrecision.Year;
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Month:
                return TimestampPrecision.Month;
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Day:
                return TimestampPrecision.Day;
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Hour:
                return TimestampPrecision.Hour;
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Minute:
                return TimestampPrecision.Minute;
            case EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Second:
                return TimestampPrecision.Second;
            default:
                throw new ArgumentOutOfRangeException(nameof(input), input, null);
            }
        }
    }
}
