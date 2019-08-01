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
    using NLog;

    internal class UpdateFileHashCommandHandler : ICancellableCommandHandler<UpdateFileHashCommand>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ISession session;

        public UpdateFileHashCommandHandler([NotNull] ISession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            this.session = session;
        }

        public async Task Handle(UpdateFileHashCommand message, CancellationToken token = new CancellationToken())
        {
            var item = await session.Get<Photo>(message.Id, message.ExpectedVersion, token).ConfigureAwait(false);
            item.UpdateFileHash(message.FileHash);
            await session.Commit(token).ConfigureAwait(false);
        }
    }
}
