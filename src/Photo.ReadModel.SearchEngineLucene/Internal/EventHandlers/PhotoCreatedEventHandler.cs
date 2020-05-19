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

    internal class PhotoCreatedEventHandler : ICancellableEventHandler<PhotoCreated>
    {
        [NotNull] private readonly IPhotoIndex photoIndex;

        public PhotoCreatedEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }

        public Task Handle([NotNull] PhotoCreated message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var photo = new Photo
            {
                Id = message.Id,
                Version = message.Version,
                FileName = message.FileName,
                FileMimeType = message.MimeType,
                DateTimeTaken = null, // todo
            };

            return photoIndex.ReIndexMediaFileAsync(photo);
        }
    }
}
