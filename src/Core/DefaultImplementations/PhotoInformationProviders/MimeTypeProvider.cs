namespace EagleEye.Core.DefaultImplementations.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.PhotoInformationProviders;

    public class MimeTypeProvider : IPhotoMimeTypeProvider
    {
        public string Name => nameof(MimeTypeProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public Task<string> ProvideAsync(string filename)
        {
            return Task.FromResult(MimeTypes.GetMimeType(filename));
        }
    }
}
