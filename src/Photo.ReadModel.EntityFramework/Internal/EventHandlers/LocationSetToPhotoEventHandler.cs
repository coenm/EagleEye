namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers
{
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
    internal class LocationSetToPhotoEventHandler : ICancellableEventHandler<LocationSetToPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEagleEyeRepository repository;

        public LocationSetToPhotoEventHandler([NotNull] IEagleEyeRepository repository)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            this.repository = repository;
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.Location, $"{nameof(message)}.{nameof(message.Location)}").NotNull();

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

        private bool VersionsMatch(int messageVersion, int currentVersion)
        {
            // todo implement this method?
            return true;
        }
    }
}
