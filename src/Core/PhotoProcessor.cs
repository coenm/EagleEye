namespace EagleEye.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;

    using Helpers.Guards;

    using JetBrains.Annotations;

    public class PhotoProcessor
    {
        [NotNull] private readonly IPhotoDateTimeTakenProvider[] dateTimeProviders;
        [NotNull] private readonly IPhotoTagProvider[] tagsProviders;
        [NotNull] private readonly IPhotoMimeTypeProvider[] mimeTypeProviders;

        public PhotoProcessor(
            [NotNull] IEnumerable<IPhotoDateTimeTakenProvider> dateTimeProviders,
            [NotNull] IEnumerable<IPhotoTagProvider> tagsProviders,
            [NotNull] IEnumerable<IPhotoMimeTypeProvider> mimeTypeProviders)
        {
            Guard.NotNull(dateTimeProviders, nameof(dateTimeProviders));
            Guard.NotNull(tagsProviders, nameof(tagsProviders));
            Guard.NotNull(mimeTypeProviders, nameof(mimeTypeProviders));

            this.dateTimeProviders = dateTimeProviders.ToArray();
            this.tagsProviders = tagsProviders.ToArray();
            this.mimeTypeProviders = mimeTypeProviders.ToArray();
        }

        [Pure]
        public async Task<Timestamp> GetTimestampAsync([NotNull] string filename, CancellationToken ct = default(CancellationToken))
        {
            Guard.NotNull(filename, nameof(filename));
            ct.ThrowIfCancellationRequested();

            Timestamp result = null;

            foreach (var provider in dateTimeProviders.OrderBy(x => x.Priority))
            {
                ct.ThrowIfCancellationRequested();

                if (provider.CanProvideInformation(filename))
                {
                    result = await provider.ProvideAsync(filename, result).ConfigureAwait(false);
                }
            }

            return result;
        }

        [Pure]
        public async Task<List<string>> GetTagsAsync([NotNull] string filename, CancellationToken ct = default(CancellationToken))
        {
            Guard.NotNull(filename, nameof(filename));
            ct.ThrowIfCancellationRequested();

            var result = new List<string>();

            foreach (var provider in tagsProviders.OrderBy(x => x.Priority))
            {
                ct.ThrowIfCancellationRequested();

                if (provider.CanProvideInformation(filename))
                {
                    result = await provider.ProvideAsync(filename, result).ConfigureAwait(false);
                }
            }

            return result;
        }

        [Pure]
        public async Task<string> GetMimeTypeAsync([NotNull] string filename, CancellationToken ct = default(CancellationToken))
        {
            Guard.NotNull(filename, nameof(filename));
            ct.ThrowIfCancellationRequested();

            var result = default(string);

            foreach (var provider in mimeTypeProviders.OrderBy(x => x.Priority))
            {
                ct.ThrowIfCancellationRequested();

                if (provider.CanProvideInformation(filename))
                {
                    result = await provider.ProvideAsync(filename, result).ConfigureAwait(false);
                }
            }

            return result;
        }
    }
}
