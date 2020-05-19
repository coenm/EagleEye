namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class LocationSetToPhotoEventHandler : ICancellableEventHandler<LocationSetToPhoto>
    {
        [NotNull] private readonly IPhotoIndex photoIndex;

        public LocationSetToPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public async Task Handle(LocationSetToPhoto message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.Location, nameof(message.Location)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
                return;

            storedItem.Version = message.Version;

            storedItem.LocationCity = message.Location.City;
            storedItem.LocationCountryCode = message.Location.CountryCode;
            storedItem.LocationCountryName = message.Location.CountryName;
            storedItem.LocationState = message.Location.State;
            storedItem.LocationSubLocation = message.Location.SubLocation;
            storedItem.LocationLatitude = message.Location.Latitude;
            storedItem.LocationLongitude = message.Location.Longitude;

            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }
    }
}
