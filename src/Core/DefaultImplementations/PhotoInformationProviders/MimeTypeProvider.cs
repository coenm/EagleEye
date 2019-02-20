namespace EagleEye.Core.DefaultImplementations.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;

    public class MimeTypeProvider : IPhotoMimeTypeProvider
    {
        public string Name => nameof(MimeTypeProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public Task<string> ProvideAsync(string filename)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            var mime = MimeTypes.GetMimeType(filename);

            return Task.FromResult(mime);
        }
    }
}
