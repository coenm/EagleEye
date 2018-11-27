﻿namespace Photo.EntityFramework.ReadModel.Internal.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    using EagleEye.Photo.Domain.Events;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;
    using Photo.EntityFramework.ReadModel.Internal.EntityFramework;
    using Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models;

    internal class MediaItemConsistency :
        ICancellableEventHandler<PhotoCreated>,
        ICancellableEventHandler<TagsAddedToPhoto>,
        ICancellableEventHandler<TagsRemovedFromPhoto>,
        ICancellableEventHandler<PersonsAddedToPhoto>,
        ICancellableEventHandler<PersonsRemovedFromPhoto>,
        ICancellableEventHandler<LocationClearedFromPhoto>,
        ICancellableEventHandler<LocationSetToPhoto>,
        ICancellableEventHandler<DateTimeTakenChanged>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEagleEyeRepository repository;

        public MediaItemConsistency([NotNull] IEagleEyeRepository repository)
        {
            Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
        }

        public async Task Handle([NotNull] PhotoCreated message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = new Photo
            {
                Id = message.Id,
                Filename = message.FileName,
                FileMimeType = message.MimeType,
                Version = message.Version,
                FileSha256 = message.FileHash,
                EventTimestamp = message.TimeStamp,
                DateTimeTaken = null,
            };

            if (message.Tags != null)
                photo.Tags = message.Tags.Select(x => new Tag { Value = x }).ToList();

            if (message.Persons != null)
                photo.People = message.Persons.Select(x => new Person { Value = x }).ToList();

            await repository.SaveAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // todo check versions?

            if (message.Tags != null && message.Tags.Any())
            {
                var origValues = photo.Tags?.Select(x => x.Value).ToList() ?? new List<string>();

                var newItems = message.Tags
                    .Where(x => origValues.All(y => x != y))
                    .Select(x => new Tag { Value = x });

                if (photo.Tags == null)
                    photo.Tags = new List<Tag>();
                photo.Tags.AddRange(newItems);
            }

            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(PersonsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // todo check versions?

            if (message.Persons != null && message.Persons.Any())
            {
                var origValues = photo.People?.Select(x => x.Value).ToList() ?? new List<string>();

                var newItems = message.Persons
                    .Where(x => origValues.All(y => x != y))
                    .Select(x => new Person { Value = x });

                if (photo.People == null)
                    photo.People = new List<Person>();
                photo.People.AddRange(newItems);
            }

            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            photo.Tags?.RemoveAll(x => message.Tags.Contains(x.Value));
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            photo.People?.RemoveAll(x => message.Persons.Contains(x.Value));
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            photo.Location = null; // not sure if this is the way to do this.
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default(CancellationToken))
        {
            DebugGuard.NotNull(message, nameof(message));
            DebugGuard.NotNull(message.Location, $"{nameof(message)}.{nameof(message.Location)}");

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            photo.Location = new EntityFramework.Models.Location()
            {
                CountryName = message.Location.CountryName,
                CountryCode = message.Location.CountryCode,
                City = message.Location.City,
                State = message.Location.State,
                SubLocation = message.Location.SubLocation,
                Latitude = message.Location.Latitude,
                Longitude = message.Location.Longitude,
            };

            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(DateTimeTakenChanged message, CancellationToken token = new CancellationToken())
        {
            DebugGuard.NotNull(message, nameof(message));

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            // check versions?

            // todo, something with precision?

            photo.DateTimeTaken = message.DateTimeTaken;

            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }
    }
}