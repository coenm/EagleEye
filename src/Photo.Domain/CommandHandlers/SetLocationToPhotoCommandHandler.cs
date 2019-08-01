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

    internal class SetLocationToPhotoCommandHandler : ICancellableCommandHandler<SetLocationToPhotoCommand>
    {
        [NotNull] private readonly ISession session;

        public SetLocationToPhotoCommandHandler([NotNull] ISession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            this.session = session;
        }

        public async Task Handle(SetLocationToPhotoCommand message, CancellationToken token)
        {
            var item = await session.Get<Photo>(message.Id, message.ExpectedVersion, token).ConfigureAwait(false);
            item.SetLocation(
                message.CountryCode,
                message.CountryName,
                message.State,
                message.City,
                message.SubLocation,
                message.Longitude,
                message.Latitude);

            await session.Commit(token).ConfigureAwait(false);
        }
    }
}
