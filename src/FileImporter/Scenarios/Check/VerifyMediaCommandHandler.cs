namespace EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class VerifyMediaCommandHandler
    {
        private readonly IFileSha256HashProvider fileSha256Service;
        private readonly IEnumerable<IPhotoSha256HashProvider> photoSha256HashProvider;
        private readonly IDateTimeService dateTimeService;
        private readonly IEagleEyeMetadataProvider eagleEyeMetadataProvider;

        public VerifyMediaCommandHandler(
            [NotNull] IFileSha256HashProvider fileSha256Service,
            [NotNull] IEnumerable<IPhotoSha256HashProvider> photoSha256HashProvider,
            [NotNull] IDateTimeService dateTimeService,
            [NotNull] IEagleEyeMetadataProvider eagleEyeMetadataProvider)
        {
            Guard.Argument(fileSha256Service, nameof(fileSha256Service)).NotNull();
            Guard.Argument(photoSha256HashProvider, nameof(photoSha256HashProvider)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            Guard.Argument(eagleEyeMetadataProvider, nameof(eagleEyeMetadataProvider)).NotNull();

            this.fileSha256Service = fileSha256Service;
            this.photoSha256HashProvider = photoSha256HashProvider;
            this.dateTimeService = dateTimeService;
            this.eagleEyeMetadataProvider = eagleEyeMetadataProvider;
        }

        private bool BytesEqual(ref byte[] bytes1, ref byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
                return true;
            if (bytes1 == null)
                return false;
            if (bytes2 == null)
                return false;

            return bytes1.SequenceEqual(bytes2);
        }

        public async Task<VerifyMediaResult> HandleAsync([NotNull] string filename, CancellationToken ct = default)
        {
            // check if file exists
            if (!File.Exists(filename))
                return new VerifyMediaResult(filename, VerifyMediaResult.MyState.FileNotExist, null);

            // check if file contains metadata
            var imageMetaData = await eagleEyeMetadataProvider.ProvideAsync(filename, ct).ConfigureAwait(false);
            if (imageMetaData == null)
                return new VerifyMediaResult(filename, VerifyMediaResult.MyState.NoMetadataAvailable, null);

            // if not -> get metadata
            var data2 = await photoSha256HashProvider.First().ProvideAsync(filename).ConfigureAwait(false);

            var rawImageHash = data2.ToArray();

            if (rawImageHash != null && rawImageHash.Length > 0)
            {
                if (imageMetaData.RawImageHash == null || imageMetaData.RawImageHash.Count == 0)
                    return new VerifyMediaResult(filename, VerifyMediaResult.MyState.MetadataIncorrect, imageMetaData);

                var found = false;
                foreach (var item in imageMetaData.RawImageHash)
                {
                    var imageMetaDataRawImageHash = item;

                    if (!found)
                        found = BytesEqual(ref rawImageHash, ref imageMetaDataRawImageHash);
                }

                if (!found)
                    return new VerifyMediaResult(filename, VerifyMediaResult.MyState.MetadataIncorrect, imageMetaData);
            }

            return new VerifyMediaResult(filename, VerifyMediaResult.MyState.MetadataCorrect, imageMetaData);
        }
    }
}
