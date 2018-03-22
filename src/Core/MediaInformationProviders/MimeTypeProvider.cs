namespace EagleEye.Core.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    public class MimeTypeProvider : IMediaInformationProvider
    {
        public int Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task ProvideAsync(string filename, MediaObject media)
        {
            var mime = MimeTypes.GetMimeType(filename);

            media.FileInformation.SetType(mime);

            return Task.CompletedTask;
        }
    }
}