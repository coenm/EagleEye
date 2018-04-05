namespace EagleEye.Core.Domain.CommandHandlers
{
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;

    using EagleEye.Core.Domain.Commands;
    using EagleEye.Core.Domain.Entities;

    public class MediaItemCommandHandlers : /*ICancellable*/ICommandHandler<CreateMediaItemCommand>
    {
        private readonly ISession _session;

        public MediaItemCommandHandlers(ISession session)
        {
            _session = session;
        }

        public async Task Handle(CreateMediaItemCommand message)
        {
            var item = new MediaItem(message.Id, message.Name);
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
    }
}