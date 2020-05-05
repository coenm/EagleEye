namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class UpdateImportImageCommandHandler
    {
        private readonly IEnumerable<IFileSha256HashProvider> _fileSha256Service;
        private readonly IEnumerable<IPhotoSha256HashProvider> _photoSha256HashProvider;
        private readonly IDateTimeService _dateTimeService;
        private readonly IEagleEyeMetadataProvider _eagleEyeMetadataProvider;

        public UpdateImportImageCommandHandler(
            // [NotNull] IEnumerable<IFileSha256HashProvider> fileSha256Service,
            // [NotNull] IEnumerable<IPhotoSha256HashProvider> photoSha256HashProvider,
            [NotNull] IDateTimeService dateTimeService,
            [NotNull] IEagleEyeMetadataProvider eagleEyeMetadataProvider)
        {
            // Guard.Argument(fileSha256Service, nameof(fileSha256Service)).NotNull();
            // Guard.Argument(photoSha256HashProvider, nameof(photoSha256HashProvider)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            Guard.Argument(eagleEyeMetadataProvider, nameof(eagleEyeMetadataProvider)).NotNull();
            // _fileSha256Service = fileSha256Service;
            // _photoSha256HashProvider = photoSha256HashProvider;
            _dateTimeService = dateTimeService;
            _eagleEyeMetadataProvider = eagleEyeMetadataProvider;
        }

        public async Task HandleAsync(string filename)
        {
            return;
            // check if file exists
            if (!File.Exists(filename))
                return;

            // check if file contains metadata
            var imageMetaData = await _eagleEyeMetadataProvider.ProvideAsync(filename).ConfigureAwait(false);
            if (imageMetaData != null)
                return;

            // if not -> get metadata
            var data = await _fileSha256Service.First().ProvideAsync(filename).ConfigureAwait(false);
            var data2 = await _photoSha256HashProvider.First().ProvideAsync(filename).ConfigureAwait(false);
            var fileId = Guid.NewGuid();

            // update metadata
            var metadata = new EagleEyeMetadata
                {
                    Id = Guid.NewGuid(),
                    FileHash = data.ToArray(),
                    Timestamp = _dateTimeService.Now,
                    RawImageHash = new List<byte[]>
                        {
                            data2.ToArray(),
                        },
                };
        }
    }
}
