namespace EagleEye.Core.Domain.CommandHandlers
{
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;

    using EagleEye.Core.Domain.Commands;
    using EagleEye.Core.Domain.Entities;

    public class MediaItemCommandHandlers : /*ICancellable*/ICommandHandler<CreateMediaItemCommand>,
                                                            ICommandHandler<AddTagsToMediaItemCommand>,
                                                            ICommandHandler<AddPersonsToMediaItemCommand>,
                                                            ICommandHandler<RemoveTagsFromMediaItemCommand>,
                                                            ICommandHandler<RemovePersonsFromMediaItemCommand>,
                                                            ICommandHandler<SetLocationToMediaItemCommand>,
                                                            ICommandHandler<ClearLocationFromMediaItemCommand>
    {
        private readonly ISession _session;

        public MediaItemCommandHandlers(ISession session)
        {
            _session = session;
        }

        public async Task Handle(CreateMediaItemCommand message)
        {
            var item = new MediaItem(message.Id, message.Name, message.Tags, message.Persons);
            await _session.Add(item).ConfigureAwait(false);
            await _session.Commit().ConfigureAwait(false);

//            var item = new MediaItem(message.Id, message.Name);
//
//            await _session.Add(item).ConfigureAwait(false);
//            await _session.Commit().ConfigureAwait(false);
//
//            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
//            item.Remove(message.Count);
//            await _session.Commit(token);
        }

        public async Task Handle(AddTagsToMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.AddTags(message.Tags);
            await _session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(RemoveTagsFromMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.RemoveTags(message.Tags);
            await _session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(AddPersonsToMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.AddPersons(message.Persons);
            await _session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(RemovePersonsFromMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.RemovePersons(message.Persons);
            await _session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(SetLocationToMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.SetLocation(
                             message.CountryCode,
                             message.CountryName,
                             message.State,
                             message.City,
                             message.SubLocation,
                             message.Longitude,
                             message.Latitude);

            await _session.Commit().ConfigureAwait(false);
        }

        public async Task Handle(ClearLocationFromMediaItemCommand message)
        {
            var item = await _session.Get<MediaItem>(message.Id).ConfigureAwait(false);
            item.ClearLocationData();
            await _session.Commit().ConfigureAwait(false);
        }
    }
}