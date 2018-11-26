namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;

    using Helpers.Guards;

    using JetBrains.Annotations;
    using NLog;

    internal class MediaItemCommandHandlers :
        ICancellableCommandHandler<CreatePhotoCommand>,
        ICancellableCommandHandler<AddTagsToPhotoCommand>,
        ICancellableCommandHandler<AddPersonsToPhotoCommand>,
        ICancellableCommandHandler<RemoveTagsFromPhotoCommand>,
        ICancellableCommandHandler<RemovePersonsFromPhotoCommand>,
        ICancellableCommandHandler<SetLocationToPhotoCommand>,
        ICancellableCommandHandler<ClearLocationFromPhotoCommand>,
        ICancellableCommandHandler<SetDateTimeTakenCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISession session;

        public MediaItemCommandHandlers([NotNull] ISession session)
        {
            Guard.NotNull(session, nameof(session));
            this.session = session;
        }

        public async Task Handle(CreatePhotoCommand message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var item = new Photo(
                message.Id,
                message.FileName,
                message.PhotoMimeType,
                message.FileSha256,
                message.Tags,
                message.Persons);

            await session.Add(item, token).ConfigureAwait(false);
            await session.Commit(token).ConfigureAwait(false);

            /*
            var item = new MediaItem(message.Id, message.Name);

            await _session.Add(item).ConfigureAwait(false);
            await _session.Commit().ConfigureAwait(false);

            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
            item.Remove(message.Count);
            await _session.Commit(token);
            */
        }

        public async Task Handle(AddTagsToPhotoCommand message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.AddTags(message.Tags);
            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(RemoveTagsFromPhotoCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.RemoveTags(message.Tags);
            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(AddPersonsToPhotoCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.AddPersons(message.Persons);
            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(RemovePersonsFromPhotoCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.RemovePersons(message.Persons);
            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(SetLocationToPhotoCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.SetLocation(
                             message.CountryCode,
                             message.CountryName,
                             message.State,
                             message.City,
                             message.SubLocation,
                             message.Longitude,
                             message.Latitude);

            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(ClearLocationFromPhotoCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.ClearLocationData();
            await session.Commit(token).ConfigureAwait(false);
        }

        public async Task Handle(SetDateTimeTakenCommand message, CancellationToken token)
        {
            var item = await Get<Photo>(message.Id, message.ExpectedVersion).ConfigureAwait(false);
            item.SetDateTimeTaken(message.DateTimeTaken.Value, ConvertTimeStampPrecision(message.DateTimeTaken.Precision));
            await session.Commit(token).ConfigureAwait(false);
        }

        private TimestampPrecision ConvertTimeStampPrecision(Commands.Inner.TimestampPrecision precision)
        {
            switch (precision)
            {
                case Commands.Inner.TimestampPrecision.Year:
                    return TimestampPrecision.Year;
                case Commands.Inner.TimestampPrecision.Month:
                    return TimestampPrecision.Month;
                case Commands.Inner.TimestampPrecision.Day:
                    return TimestampPrecision.Day;
                case Commands.Inner.TimestampPrecision.Hour:
                    return TimestampPrecision.Hour;
                case Commands.Inner.TimestampPrecision.Minute:
                    return TimestampPrecision.Minute;
                case Commands.Inner.TimestampPrecision.Second:
                    return TimestampPrecision.Second;
                default:
                    throw new ArgumentOutOfRangeException(nameof(precision), precision, null);
            }
        }

        private async Task<T> Get<T>(Guid id, int? expectedVersion = null)
            where T : AggregateRoot
        {
            try
            {
                return await session.Get<T>(id, expectedVersion).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Logger.Error("Cannot get object of type {0} with id:{1} ({2}) from session", typeof(T), id, expectedVersion);
                throw;
            }
        }
    }
}
