namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using Dawn;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.CommandHandlers.Exceptions;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Services;
    using JetBrains.Annotations;

    internal class CreatePhotoCommandHandler : ICancellableCommandHandler<CreatePhotoCommand>
    {
        [NotNull] private readonly ISession session;
        [NotNull] private readonly IUniqueFilenameService uniqueFilenameService;

        public CreatePhotoCommandHandler(
            [NotNull] ISession session,
            [NotNull] IUniqueFilenameService uniqueFilenameService)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            Guard.Argument(uniqueFilenameService, nameof(uniqueFilenameService)).NotNull();
            this.session = session;
            this.uniqueFilenameService = uniqueFilenameService;
        }

        public async Task Handle(CreatePhotoCommand message, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var item = new Photo(
                message.Id,
                message.FileName,
                message.PhotoMimeType,
                message.FileSha256);

            var filenameClaim = uniqueFilenameService.Claim(message.FileName);
            if (filenameClaim == null)
                throw new PhotoAlreadyExistsException(message.FileName);

            using (filenameClaim)
            {
                await session.Add(item, token).ConfigureAwait(false);
                await session.Commit(token).ConfigureAwait(false);

                filenameClaim.Commit();
            }
        }
    }
}
