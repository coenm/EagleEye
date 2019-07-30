namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using Dawn;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;
    using JetBrains.Annotations;

    internal class UpdatePhotoHashCommandHandler : ICancellableCommandHandler<UpdatePhotoHashCommand>
    {
        [NotNull] private readonly ISession session;

        public UpdatePhotoHashCommandHandler([NotNull] ISession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            this.session = session;
        }

        public async Task Handle(UpdatePhotoHashCommand message, CancellationToken token)
        {
            var item = await session.Get<Photo>(message.Id, message.ExpectedVersion, token).ConfigureAwait(false);
            item.UpdatePhotoHash(message.HashIdentifier, message.PhotoHash);
            await session.Commit(token).ConfigureAwait(false);
        }
    }
}
