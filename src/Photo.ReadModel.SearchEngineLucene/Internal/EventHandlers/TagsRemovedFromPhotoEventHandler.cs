namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    internal class TagsRemovedFromPhotoEventHandler : ICancellableEventHandler<TagsRemovedFromPhoto>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IPhotoIndex photoIndex;

        public TagsRemovedFromPhotoEventHandler([NotNull] IPhotoIndex photoIndex)
        {
            Guard.Argument(photoIndex, nameof(photoIndex)).NotNull();
            this.photoIndex = photoIndex;
        }


        public async Task Handle(TagsRemovedFromPhoto message, CancellationToken token = new CancellationToken())
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.Tags, nameof(message.Tags)).NotNull();

            if (!(photoIndex.Search(message.Id) is Photo storedItem))
                return;

            storedItem.Version = message.Version;
            if (storedItem.Tags == null)
                return;

            if (!storedItem.Tags.Any(t => message.Tags.Contains(t)))
                return;

            storedItem.Tags.RemoveAll(t => message.Tags.Contains(t));
            await photoIndex.ReIndexMediaFileAsync(storedItem).ConfigureAwait(false);
        }
    }
}
