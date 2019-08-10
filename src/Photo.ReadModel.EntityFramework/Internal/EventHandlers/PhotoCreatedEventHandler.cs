namespace EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    internal class PhotoCreatedEventHandler : ICancellableEventHandler<PhotoCreated>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly IEagleEyeRepository repository;

        public PhotoCreatedEventHandler([NotNull] IEagleEyeRepository repository)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            this.repository = repository;
        }

        public async Task Handle([NotNull] PhotoCreated message, CancellationToken token = default)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var photo = new Photo
            {
                Id = message.Id,
                Filename = message.FileName,
                FileMimeType = message.MimeType,
                Version = message.Version,
                FileSha256 = message.FileHash,
                EventTimestamp = message.TimeStamp,
                DateTimeTaken = null,
            };

            await repository.SaveAsync(photo).ConfigureAwait(false);
        }
    }
}
