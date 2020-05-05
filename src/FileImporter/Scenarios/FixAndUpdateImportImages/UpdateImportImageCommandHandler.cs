namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class UpdateImportImageCommandHandler
    {
        private readonly IFileSha256HashProvider _fileSha256Service;
        private readonly IPhotoSha256HashProvider _photoSha256HashProvider;
        private readonly IDateTimeService _dateTimeService;

        public UpdateImportImageCommandHandler(
            [NotNull] IFileSha256HashProvider fileSha256Service,
            [NotNull] IPhotoSha256HashProvider photoSha256HashProvider,
            [NotNull] IDateTimeService dateTimeService)
        {
            Guard.Argument(fileSha256Service, nameof(fileSha256Service)).NotNull();
            Guard.Argument(photoSha256HashProvider, nameof(photoSha256HashProvider)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            _fileSha256Service = fileSha256Service;
            _photoSha256HashProvider = photoSha256HashProvider;
            _dateTimeService = dateTimeService;
        }

        public async Task HandleAsync(string filename)
        {
            // check if file exists
            if (!File.Exists(filename))
                return;

            // check if file contains metadata
            // var imageMetaData = await .GetMetadata(filename).ConfigureAwait(false);

            // if not -> get metadata
            var data = await _fileSha256Service.ProvideAsync(filename).ConfigureAwait(false);
            var data2 = await _photoSha256HashProvider.ProvideAsync(filename).ConfigureAwait(false);
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
