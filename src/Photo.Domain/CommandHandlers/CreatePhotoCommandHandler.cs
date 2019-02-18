namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Services;
    using Helpers.Guards;
    using JetBrains.Annotations;

    internal class CreatePhotoCommandHandler : ICancellableCommandHandler<CreatePhotoCommand>
    {
        [NotNull] private readonly ISession session;
        [NotNull] private readonly IUniqueFilenameService uniqueFilenameService;

        public CreatePhotoCommandHandler(
            [NotNull] ISession session,
            [NotNull] IUniqueFilenameService uniqueFilenameService)
        {
            Guard.NotNull(session, nameof(session));
            Guard.NotNull(uniqueFilenameService, nameof(uniqueFilenameService));
            this.session = session;
            this.uniqueFilenameService = uniqueFilenameService;
        }

        public async Task Handle(CreatePhotoCommand message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var claim = uniqueFilenameService.Claim(message.FileName);
            if (claim == null)
                throw new Exception();

            try
            {
                var item = new Photo(
                    message.Id,
                    message.FileName,
                    message.PhotoMimeType,
                    message.FileSha256);

                await session.Add(item, token).ConfigureAwait(false);
                await session.Commit(token).ConfigureAwait(false);

                claim.Commit();

                /*
                var item = new MediaItem(message.Id, message.Name);

                await _session.Add(item).ConfigureAwait(false);
                await _session.Commit().ConfigureAwait(false);

                var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
                item.Remove(message.Count);
                await _session.Commit(token);
                */
            }
            finally
            {
                claim.Dispose();
            }
        }
    }
}
