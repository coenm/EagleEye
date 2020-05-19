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
    internal class LocationClearedFromPhotoEventHandler : ICancellableEventHandler<LocationClearedFromPhoto>
    {
        [NotNull] private readonly IPhotoIndex photoIndex;

        public LocationClearedFromPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public async Task Handle(LocationClearedFromPhoto message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
                return;

            storedItem.Version = message.Version;

            storedItem.LocationCity = null;
            storedItem.LocationCountryCode = null;
            storedItem.LocationCountryName = null;
            storedItem.LocationState = null;
            storedItem.LocationSubLocation = null;
            storedItem.LocationLatitude = null;
            storedItem.LocationLongitude = null;

            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }
    }
}
