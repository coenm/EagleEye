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

    [UsedImplicitly]
    internal class PersonsAddedToPhotoEventHandler : ICancellableEventHandler<PersonsAddedToPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEagleEyeRepository repository;

        public PersonsAddedToPhotoEventHandler([NotNull] IEagleEyeRepository repository)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            this.repository = repository;
        }

        public async Task Handle(PersonsAddedToPhoto message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();

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

        private bool VersionsMatch(int messageVersion, int currentVersion)
        {
            // todo implement this method?
            return true;
        }
    }
}
