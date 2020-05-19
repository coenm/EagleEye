namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class DateTimeTakenChangedEventHandler : ICancellableEventHandler<DateTimeTakenChanged>
    {
        [NotNull] private readonly IPhotoIndex photoIndex;

        public DateTimeTakenChangedEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public async Task Handle(DateTimeTakenChanged message, CancellationToken token = new CancellationToken())
        {
            Guard.Argument(message, nameof(message)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
                return;

            storedItem.Version = message.Version;
            storedItem.DateTimeTaken = new Timestamp(message.DateTimeTaken.Value, Convert(message.DateTimeTaken.Precision));

            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }

        private TimestampPrecision Convert(EagleEye.Photo.Domain.Aggregates.TimestampPrecision input)
        {
            return input switch
            {
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Year => TimestampPrecision.Year,
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Month => TimestampPrecision.Month,
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Day => TimestampPrecision.Day,
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Hour => TimestampPrecision.Hour,
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Minute => TimestampPrecision.Minute,
                EagleEye.Photo.Domain.Aggregates.TimestampPrecision.Second => TimestampPrecision.Second,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
        }
    }
}
