namespace EagleEye.Core.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using Helpers.Guards;
    using JetBrains.Annotations;

    public class MimeTypeProvider : IMediaInformationProvider
    {
        public int Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            return true;
        }

        public Task ProvideAsync(string filename, [NotNull] MediaObject media)
        {
            Guard.NotNull(media, nameof(media));

            var mime = MimeTypes.GetMimeType(filename);

            media.FileInformation.SetType(mime);

            return Task.CompletedTask;
        }
    }
}
