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

    internal class SetDateTimeTakenCommandHandler : ICancellableCommandHandler<SetDateTimeTakenCommand>
    {
        [NotNull] private readonly ISession session;

        public SetDateTimeTakenCommandHandler([NotNull] ISession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            this.session = session;
        }

        public async Task Handle(SetDateTimeTakenCommand message, CancellationToken token)
        {
            var item = await session.Get<Photo>(message.Id, message.ExpectedVersion, token).ConfigureAwait(false);
            item.SetDateTimeTaken(
                message.DateTimeTaken.Year,
                message.DateTimeTaken.Month,
                message.DateTimeTaken.Day,
                message.DateTimeTaken.Hour,
                message.DateTimeTaken.Minutes,
                message.DateTimeTaken.Seconds);
            await session.Commit(token).ConfigureAwait(false);
        }
    }
}
