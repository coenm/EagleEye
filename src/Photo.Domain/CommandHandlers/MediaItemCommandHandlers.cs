namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;

    using Helpers.Guards;

    using JetBrains.Annotations;

    internal class MediaItemCommandHandlers : /*ICancellable*/ICommandHandler<CreatePhotoCommand>,
                                                            ICommandHandler<AddTagsToPhotoCommand>,
                                                            ICommandHandler<AddPersonsToPhotoCommand>,
                                                            ICommandHandler<RemoveTagsFromPhotoCommand>,
                                                            ICommandHandler<RemovePersonsFromPhotoCommand>,
                                                            ICommandHandler<SetLocationToPhotoCommand>,
                                                            ICommandHandler<ClearLocationFromPhotoCommand>,
        ICommandHandler<SetDateTimeTakenCommand>
    {
        private readonly ISession session;

        public MediaItemCommandHandlers([NotNull] ISession session)
        {
            Guard.NotNull(session, nameof(session));
            this.session = session;
        }

        public async Task Handle(CreatePhotoCommand message)
        {
            var item = new Photo(
                message.Id,
                message.FileName,
                message.PhotoMimeType,
                message.FileSha256,
                message.Tags,
                message.Persons);

            await session.Add(item).ConfigureAwait(false);
            await session.Commit().ConfigureAwait(false);

            /*
            var item = new MediaItem(message.Id, message.Name);

            await _session.Add(item).ConfigureAwait(false);
            await _session.Commit().ConfigureAwait(false);

            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
            item.Remove(message.Count);
            await _session.Commit(token);
            */
        }

        public async Task Handle(AddTagsToPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.AddTags(message.Tags);
            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(RemoveTagsFromPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.RemoveTags(message.Tags);
            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(AddPersonsToPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.AddPersons(message.Persons);
            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(RemovePersonsFromPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.RemovePersons(message.Persons);
            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(SetLocationToPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.SetLocation(
                             message.CountryCode,
                             message.CountryName,
                             message.State,
                             message.City,
                             message.SubLocation,
                             message.Longitude,
                             message.Latitude);

            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(ClearLocationFromPhotoCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.ClearLocationData();
            await session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(SetDateTimeTakenCommand message)
        {
            var item = await session.Get<Photo>(message.Id).ConfigureAwait(false);
            item.SetDateTimeTaken(message.DateTimeTaken.Value, ConvertTimeStampPrecision(message.DateTimeTaken.Precision));
            await session.Commit().ConfigureAwait(false);
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
    }
}
