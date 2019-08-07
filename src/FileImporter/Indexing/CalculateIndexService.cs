namespace EagleEye.FileImporter.Indexing
{
    using System.Collections.Generic;

    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    // todo should be removed.

    public class CalculateIndexService
    {
        private readonly IPhotoHashProvider photoHashProvider;
        private readonly IPhotoSha256HashProvider photoSha256HashProvider;
        private readonly IFileSha256HashProvider fileSha256HashProvider;

        public CalculateIndexService(
            [NotNull] IFileSha256HashProvider fileSha256HashProvider,
            [NotNull] IPhotoHashProvider photoHashProvider,
            [NotNull] IPhotoSha256HashProvider photoSha256HashProvider)
        {
            Guard.Argument(fileSha256HashProvider, nameof(fileSha256HashProvider)).NotNull();
            Guard.Argument(photoHashProvider, nameof(photoHashProvider)).NotNull();
            Guard.Argument(photoSha256HashProvider, nameof(photoSha256HashProvider)).NotNull();

            this.fileSha256HashProvider = fileSha256HashProvider;
            this.photoHashProvider = photoHashProvider;
            this.photoSha256HashProvider = photoSha256HashProvider;
        }

        public IEnumerable<ImageData> CalculateIndex(IReadOnlyList<string> fileIdentifiers)
        {
            Guard.Argument(fileIdentifiers, nameof(fileIdentifiers)).NotNull();

            var result = new ImageData[fileIdentifiers.Count];

            for (var index = 0; index < fileIdentifiers.Count; index++)
            {
                var h = photoHashProvider.ProvideAsync(fileIdentifiers[index]).GetAwaiter().GetResult();
                var ih = photoSha256HashProvider.ProvideAsync(fileIdentifiers[index]).GetAwaiter().GetResult();
                var fh = fileSha256HashProvider.ProvideAsync(fileIdentifiers[index]).GetAwaiter().GetResult();

                var hashes = new ImageHashValues
                {
                    FileHash = fh.ToArray(),
                    ImageHash = ih.ToArray(),
                };

                foreach (var item in h)
                {
                    if (item.HashName == nameof(hashes.AverageHash))
                        hashes.AverageHash = item.Hash;
                    else if (item.HashName == nameof(hashes.DifferenceHash))
                        hashes.DifferenceHash = item.Hash;
                    else if (item.HashName == nameof(hashes.PerceptualHash))
                        hashes.PerceptualHash = item.Hash;
                }

                result[index] = new ImageData(fileIdentifiers[index])
                {
                    Hashes =  hashes
                };
            }

            return result;
        }
    }
}
