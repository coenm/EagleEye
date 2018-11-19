namespace EagleEye.Core.ReadModel.EventHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;

    using EagleEye.Core.Domain.Events;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Models;

    public class MediaItemConsistency :
        ICancellableEventHandler<MediaItemCreated>,
        ICancellableEventHandler<TagsAddedToMediaItem>,
        ICancellableEventHandler<TagsRemovedFromMediaItem>,
        ICancellableEventHandler<PersonsAddedToMediaItem>,
        ICancellableEventHandler<PersonsRemovedFromMediaItem>,
        ICancellableEventHandler<LocationClearedFromMediaItem>,
        ICancellableEventHandler<LocationSetToMediaItem>
    {
        private readonly IEagleEyeRepository repository;

        public MediaItemConsistency(IEagleEyeRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(MediaItemCreated message, CancellationToken token = default(CancellationToken))
        {
            var mediaItemDto = new Photo
            {
                Id = message.Id,
                Filename = message.FileName,
                Version = message.Version,
                FileSha256 = new byte[0],
                EventTimestamp = message.TimeStamp,
            };

            if (message.Tags != null)
                mediaItemDto.Tags = new List<Tag>(); // todo message.Tags

            if (message.Persons != null)
                mediaItemDto.People = new List<Person>(); // todo (message.Persons);


//            var item = new MediaItemDb
//                           {
//                               Filename = message.FileName,
//                               Id = message.Id,
//                               Version = message.Version,
//                               TimeStampUtc = message.TimeStamp,
//                               SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto),
//                           };

            await repository.SaveAsync(mediaItemDto).ConfigureAwait(false);
        }

        public async Task Handle(TagsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            // todo update tags..
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

//            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);
//
//            if (mediaItemDto.Tags == null)
//                mediaItemDto.Tags = new List<string>();
//
//            mediaItemDto.Tags.AddRange(message.Tags);
//
//            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsAddedToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            //todo add persons.

//            var mediaItemDto = JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto);
//
//            if (mediaItemDto.Persons == null)
//                mediaItemDto.Persons = new List<string>();
//
//            mediaItemDto.Persons.AddRange(message.Persons);
//
//            item.SerializedMediaItemDto = JsonConvert.SerializeObject(mediaItemDto);

            // update tags
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(TagsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            item.Tags?.RemoveAll(x => message.Tags.Contains(x.Value));
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(PersonsRemovedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            item.People?.RemoveAll(x => message.Persons.Contains(x.Name));
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationClearedFromMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            item.Location = null; // not sure if this is the way to do this.

            // update tags
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }

        public async Task Handle(LocationSetToMediaItem message, CancellationToken token = default(CancellationToken))
        {
            var item = await repository.GetByIdAsync(message.Id).ConfigureAwait(false);

            if (item == null)
                return; // throw??

            // check versions?

            item.Location = new Location
            {
                CountryName = message.Location.CountryName,
                CountryCode = message.Location.CountryCode,
                City = message.Location.City,
                State = message.Location.State,
                SubLocation = message.Location.SubLocation,
                Latitude = message.Location.Latitude,
                Longitude = message.Location.Longitude,
            };

            // update tags
            item.EventTimestamp = message.TimeStamp;
            item.Version = message.Version;

            await repository.UpdateAsync(item).ConfigureAwait(false);
        }
    }
}
