namespace EagleEye.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;

    using JetBrains.Annotations;

    public class PhotoProcessor
    {
        [NotNull] private readonly IEnumerable<IPhotoDateTimeTakenProvider> dateTimeProviders;
        [NotNull] private readonly IEnumerable<IPhotoTagProvider> tagsProviders;
        [NotNull] private readonly IEnumerable<IPhotoMimeTypeProvider> mimeTypeProviders;

        public PhotoProcessor(
            [NotNull] IEnumerable<IPhotoDateTimeTakenProvider> dateTimeProviders,
            [NotNull] IEnumerable<IPhotoTagProvider> tagsProviders,
            [NotNull] IEnumerable<IPhotoMimeTypeProvider> mimeTypeProviders)
        {
            Guard.Argument(dateTimeProviders, nameof(dateTimeProviders)).NotNull();
            Guard.Argument(tagsProviders, nameof(tagsProviders)).NotNull();
            Guard.Argument(mimeTypeProviders, nameof(mimeTypeProviders)).NotNull();

            this.dateTimeProviders = dateTimeProviders;
            this.tagsProviders = tagsProviders;
            this.mimeTypeProviders = mimeTypeProviders;
        }

        [Pure]
        public async Task<Timestamp> GetTimestampAsync([NotNull] string filename, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
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
        public async Task<List<string>> GetTagsAsync([NotNull] string filename, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
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
        public async Task<string> GetMimeTypeAsync([NotNull] string filename, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull();
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
