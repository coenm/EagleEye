namespace EagleEye.Core.ReadModel.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    using EagleEye.Core.Domain.Events;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    public class MediaItemConsistancy :
        ICancellableEventHandler<MediaItemCreated>,
        ICancellableEventHandler<TagsAddedToMediaItem>
    {
        private readonly IMediaItemRepository _repository;

        public MediaItemConsistancy(IMediaItemRepository repository)
        {
            _repository = repository;
        }

        public Task Handle(MediaItemCreated message, CancellationToken token = new CancellationToken())
        {
            var item = new MediaItemDb
                           {
                               Filename = message.Name,
                               Id = message.Id,
                               Version = message.Version,
                               TimeStampUtc = message.TimeStamp,
                               SerializedData = "todo"
                           };

            return _repository.SaveAsync(item);
        }

        public async Task Handle(TagsAddedToMediaItem message, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            // update tags

            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await _repository.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}