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

    public class MediaItemConsistency :
        ICancellableEventHandler<MediaItemCreated>,
        ICancellableEventHandler<TagsAddedToMediaItem>,
        ICancellableEventHandler<TagsRemovedFromMediaItem>,
        ICancellableEventHandler<PersonsAddedToMediaItem>,
        ICancellableEventHandler<PersonsRemovedFromMediaItem>,
        ICancellableEventHandler<LocationClearedFromMediaItem>,
        ICancellableEventHandler<LocationSetToMediaItem>
    {
        private readonly IMediaItemRepository repository;

        public MediaItemConsistency(IMediaItemRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(MediaItemCreated message, CancellationToken token = default(CancellationToken))
        {
            var mediaItemDto = new MediaItemDto();
            if (message.Tags != null)
                mediaItemDto.Tags = new List<string>(message.Tags);

            if (message.Persons!= null)
                mediaItemDto.Persons = new List<string>(message.Persons);

            var item = new MediaItemDb
                           {
                               Filename = message.FileName,
                               Id = message.Id,
                               Version = message.Version,
                               TimeStampUtc = message.TimeStamp,
                               SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto)
                           };

            await repository.SaveAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

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

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

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

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

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

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

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

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            if (mediaItemDto.Location == null)
                return; // throw?

            mediaItemDto.Location = null;

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);

            mediaItemDto.Location = new LocationDto
                                        {
                                            CountryName = message.Location.CountryName,
                                            CountryCode = message.Location.CountryCode,
                                            City = message.Location.City,
                                            State = message.Location.State,
                                            SubLocation = message.Location.SubLocation,
                                            Latitude = message.Location.Latitude,
                                            Longitude = message.Location.Longitude,
                                        };

            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.Version = message.Version;
            item.TimeStampUtc = message.TimeStamp;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}