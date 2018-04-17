namespace EagleEye.Core.ReadModel.EventHandlers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    using EagleEye.Core.Domain.Events;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using Newtonsoft.Json;

    public class MediaItemConsistancy :
        ICancellableEventHandler<MediaItemCreated>,
        ICancellableEventHandler<TagsAddedToMediaItem>,
        ICancellableEventHandler<TagsRemovedFromMediaItem>,
        ICancellableEventHandler<PersonsAddedToMediaItem>,
        ICancellableEventHandler<PersonsRemovedFromMediaItem>
    {
        private readonly IMediaItemRepository _repository;

        public MediaItemConsistancy(IMediaItemRepository repository)
        {
            _repository = repository;
        }

        public async Task Handle(MediaItemCreated message, CancellationToken token = new CancellationToken())
        {
            var mediaItemDto = new MediaItemDto();
            if (message.Tags != null)
                mediaItemDto.Tags = new List<string>(message.Tags);

            if (message.Persons!= null)
                mediaItemDto.Persons = new List<string>(message.Persons);

            var item = new MediaItemDb
                           {
                               Filename = message.Name,
                               Id = message.Id,
                               Version = message.Version,
                               TimeStampUtc = message.TimeStamp,
                               SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto)
                           };

            await _repository.SaveAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToMediaItem message, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            if (mediaItemDto.Tags == null)
                mediaItemDto.Tags = new List<string>();

            mediaItemDto.Tags.AddRange(message.Tags);

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await _repository.UpdateAsync(item).ConfigureAwait(false);
        }


        public async Task Handle(PersonsAddedToMediaItem message, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            if (mediaItemDto.Persons == null)
                mediaItemDto.Persons = new List<string>();

            mediaItemDto.Persons.AddRange(message.Persons);

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await _repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromMediaItem message, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            if (mediaItemDto.Tags == null)
                return; // throw?

            foreach (var tag in message.Tags)
            {
                mediaItemDto.Tags.Remove(tag);
            }

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await _repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromMediaItem message, CancellationToken token = new CancellationToken())
        {
            var item = await _repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            if (mediaItemDto.Tags == null)
                return; // throw?

            foreach (var person in message.Persons)
            {
                mediaItemDto.Persons.Remove(person);
            }

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await _repository.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}