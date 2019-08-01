namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System;
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
            item.SetDateTimeTaken(message.DateTimeTaken.Value, ConvertTimeStampPrecision(message.DateTimeTaken.Precision));
            await session.Commit(token).ConfigureAwait(false);
        }

        private TimestampPrecision ConvertTimeStampPrecision(Commands.Inner.TimestampPrecision precision)
        {
            switch (precision)
            {
            case Commands.Inner.TimestampPrecision.Year:
                return TimestampPrecision.Year;
            case Commands.Inner.TimestampPrecision.Month:
                return TimestampPrecision.Month;
            case Commands.Inner.TimestampPrecision.Day:
                return TimestampPrecision.Day;
            case Commands.Inner.TimestampPrecision.Hour:
                return TimestampPrecision.Hour;
            case Commands.Inner.TimestampPrecision.Minute:
                return TimestampPrecision.Minute;
            case Commands.Inner.TimestampPrecision.Second:
                return TimestampPrecision.Second;
            default:
                throw new ArgumentOutOfRangeException(nameof(precision), precision, null);
            }
        }
    }
}
