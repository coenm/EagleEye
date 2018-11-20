namespace EagleEye.Core.ReadModel.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    using EagleEye.Core.Domain.Events;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Models;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;

    internal class MediaItemConsistency :
        ICancellableEventHandler<MediaItemCreated>,
        ICancellableEventHandler<TagsAddedToMediaItem>,
        ICancellableEventHandler<TagsRemovedFromMediaItem>,
        ICancellableEventHandler<PersonsAddedToMediaItem>,
        ICancellableEventHandler<PersonsRemovedFromMediaItem>,
        ICancellableEventHandler<LocationClearedFromMediaItem>,
        ICancellableEventHandler<LocationSetToMediaItem>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEagleEyeRepository repository;

        public MediaItemConsistency(IEagleEyeRepository repository)
        {
            Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
        }

        public async Task Handle([NotNull] MediaItemCreated message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var mediaItemDto = new Photo
            {
                Id = message.Id,
                Filename = message.FileName,
                Version = message.Version,
                FileSha256 = new byte[0],
                EventTimestamp = message.TimeStamp,
            };

            if (message.Tags != null)
                mediaItemDto.Tags = message.Tags.Select(x => new Tag { Value = x }).ToList();

            if (message.Persons != null)
                mediaItemDto.People = message.Persons.Select(x => new Person { Value = x }).ToList();

            await repository.SaveAsync(mediaItemDto).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // todo check versions?

            if (message.Tags != null && message.Tags.Any())
            {
                var origValues = item.Tags.Select(x => x.Value).ToList();

                var newItems = message.Tags
                    .Where(x => origValues.All(y => x != y))
                    .Select(x => new Tag { Value = x });

                item.Tags.AddRange(newItems);
            }

            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // todo check versions?

            if (message.Persons != null && message.Persons.Any())
            {
                var origValues = item.People.Select(x => x.Value).ToList();

                var newItems = message.Persons
                    .Where(x => origValues.All(y => x != y))
                    .Select(x => new Person { Value = x });

                item.People.AddRange(newItems);
            }

            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            item.Tags?.RemoveAll(x => message.Tags.Contains(x.Value));
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            item.People?.RemoveAll(x => message.Persons.Contains(x.Value));
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            item.Location = null; // not sure if this is the way to do this.
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));
            DebugGuard.NotNull(message.Location, $"{nameof(message)}.{nameof(message.Location)}");

            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            item.Location = new Location
            {
                CountryName = message.Location.CountryName,
                CountryCode = message.Location.CountryCode,
                City = message.Location.City,
                State = message.Location.State,
                SubLocation = message.Location.SubLocation,
                Latitude = message.Location.Latitude,
                Longitude = message.Location.Longitude,
            };

            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}
