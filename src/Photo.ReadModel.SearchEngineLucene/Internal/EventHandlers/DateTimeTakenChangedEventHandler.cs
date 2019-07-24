﻿namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
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
    using NLog;

    [UsedImplicitly]
    internal class DateTimeTakenChangedEventHandler : ICancellableEventHandler<DateTimeTakenChanged>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
            storedItem.DateTimeTaken = new Timestamp(message.DateTimeTaken, Convert(message.Precision));

            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
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