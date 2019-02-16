namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using NLog;

    internal class CreatePhotoCommandHandler : ICancellableCommandHandler<CreatePhotoCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISession session;

        public CreatePhotoCommandHandler([NotNull] ISession session)
        {
            Guard.NotNull(session, nameof(session));
            this.session = session;
        }

        public async Task Handle(CreatePhotoCommand message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var item = new Photo(
                message.Id,
                message.FileName,
                message.PhotoMimeType,
                message.FileSha256,
                message.Tags,
                message.Persons);

            await session.Add(item, token).ConfigureAwait(false);
            await session.Commit(token).ConfigureAwait(false);

            /*
            var item = new MediaItem(message.Id, message.Name);

            await _session.Add(item).ConfigureAwait(false);
            await _session.Commit().ConfigureAwait(false);

            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
            item.Remove(message.Count);
            await _session.Commit(token);
            */
        }
    }
}
