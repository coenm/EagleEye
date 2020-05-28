namespace EagleEye.Photo.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.Module;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.Domain.Services;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class ModuleInitializer : IEagleEyeInitialize
    {
        [NotNull] private readonly IMediaFilenameRepository mediaRepository;
        [NotNull] private readonly IEventExporter eventExporter;

        public ModuleInitializer([NotNull] IMediaFilenameRepository mediaRepository, [NotNull] IEventExporter eventExporter)
        {
            Guard.Argument(mediaRepository, nameof(mediaRepository)).NotNull();
            Guard.Argument(eventExporter, nameof(eventExporter)).NotNull();
            this.mediaRepository = mediaRepository;
            this.eventExporter = eventExporter;
        }

        public async Task InitializeAsync()
        {
            var events = await eventExporter.GetAsync(DateTime.MinValue, CancellationToken.None).ConfigureAwait(false);

            foreach (var evt in events)
            {
                if (evt is PhotoCreated photoCreatedEvent)
                    mediaRepository.Add(photoCreatedEvent.FileName);
            }
        }
    }
}
