﻿namespace EagleEye.Photo.Domain.CommandHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using Dawn;
    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Commands;
    using JetBrains.Annotations;

    internal class ClearLocationFromPhotoCommandHandler : ICancellableCommandHandler<ClearLocationFromPhotoCommand>
    {
        [NotNull] private readonly ISession session;

        public ClearLocationFromPhotoCommandHandler([NotNull] ISession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();
            this.session = session;
        }

        public async Task Handle(ClearLocationFromPhotoCommand message, CancellationToken token)
        {
            var item = await session.Get<Photo>(message.Id, message.ExpectedVersion, token).ConfigureAwait(false);
            item.ClearLocationData();
            await session.Commit(token).ConfigureAwait(false);
        }
    }
}
