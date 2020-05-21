namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class UpdateImportImageCommandHandler : IUpdateImportImageCommandHandler
    {
        private readonly IFileSha256HashProvider fileSha256Service;
        private readonly IEnumerable<IPhotoSha256HashProvider> photoSha256HashProvider;
        private readonly IDateTimeService dateTimeService;
        private readonly IEagleEyeMetadataProvider eagleEyeMetadataProvider;
        private readonly IEagleEyeMetadataWriter eagleEyeMetadataWriter;

        public UpdateImportImageCommandHandler(
            [NotNull] IFileSha256HashProvider fileSha256Service,
            [NotNull] IEnumerable<IPhotoSha256HashProvider> photoSha256HashProvider,
            [NotNull] IDateTimeService dateTimeService,
            [NotNull] IEagleEyeMetadataProvider eagleEyeMetadataProvider,
            [NotNull] IEagleEyeMetadataWriter eagleEyeMetadataWriter)
        {
            Guard.Argument(fileSha256Service, nameof(fileSha256Service)).NotNull();
            Guard.Argument(photoSha256HashProvider, nameof(photoSha256HashProvider)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            Guard.Argument(eagleEyeMetadataProvider, nameof(eagleEyeMetadataProvider)).NotNull();
            Guard.Argument(eagleEyeMetadataWriter, nameof(eagleEyeMetadataWriter)).NotNull();

            this.fileSha256Service = fileSha256Service;
            this.photoSha256HashProvider = photoSha256HashProvider;
            this.dateTimeService = dateTimeService;
            this.eagleEyeMetadataProvider = eagleEyeMetadataProvider;
            this.eagleEyeMetadataWriter = eagleEyeMetadataWriter;
        }

        public async Task HandleAsync([NotNull] string filename, CancellationToken ct = default)
        {
            // check if file exists
            if (!File.Exists(filename))
                return;

            // check if file contains metadata
            var imageMetaData = await eagleEyeMetadataProvider.ProvideAsync(filename, ct).ConfigureAwait(false);
            if (imageMetaData != null)
                return;

            // if not -> get metadata
            var data = await fileSha256Service.ProvideAsync(filename).ConfigureAwait(false);
            var data2 = await photoSha256HashProvider.First().ProvideAsync(filename).ConfigureAwait(false);

            var metadata = new EagleEyeMetadata
                {
                    Id = Guid.NewGuid(),
                    FileHash = data.ToArray(),
                    Timestamp = dateTimeService.Now,
                    RawImageHash = new List<byte[]>
                        {
                            data2.ToArray(),
                        },
                };

            await eagleEyeMetadataWriter.WriteAsync(filename, metadata, overwriteOriginal: true, ct).ConfigureAwait(false);
        }
    }
}
