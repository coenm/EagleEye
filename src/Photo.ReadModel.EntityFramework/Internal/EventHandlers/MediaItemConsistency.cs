namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using JetBrains.Annotations;
    using NLog;

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
            Dawn.Guard.Argument(repository, nameof(repository)).NotNull();
            this.repository = repository;
        }

        public async Task Handle([NotNull] PhotoCreated message, CancellationToken token = default(CancellationToken))
        {
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

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

            await repository.SaveAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToPhoto message, CancellationToken token = default(CancellationToken))
        {
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

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
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

            if (message.Persons.Any())
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
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

            photo.Tags?.RemoveAll(x => message.Tags.Contains(x.Value));
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

            photo.People?.RemoveAll(x => message.Persons.Contains(x.Value));
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromPhoto message, CancellationToken token = default(CancellationToken))
        {
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

            photo.Location = null; // not sure if this is the way to do this.
            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default(CancellationToken))
        {
            Dawn.Guard.Argument(message, nameof(message)).NotNull();
            Dawn.Guard.Argument(message.Location, $"{nameof(message)}.{nameof(message.Location)}").NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

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
            Dawn.Guard.Argument(message, nameof(message)).NotNull();

            var photo = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (photo == null)
            {
                Logger.Error($"No {nameof(Photo)} found with id {message.Id}.");
                return;
            }

            if (!VersionsMatch(message.Version, photo.Version))
                return;

            // todo, something with precision?
            photo.DateTimeTaken = message.DateTimeTaken;

            photo.EventTimestamp = message.TimeStamp;
            photo.Version = message.Version;

            await repository.UpdateAsync(photo).ConfigureAwait(false);
        }

        private bool VersionsMatch(int messageVersion, int currentVersion)
        {
            // todo implement this method?
            return true;
        }
    }
}
