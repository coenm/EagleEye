namespace EagleEye.Core.DefaultImplementations.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;

    public class MimeTypeProvider : IPhotoMimeTypeProvider
    {
        public string Name => nameof(MimeTypeProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public Task<string> ProvideAsync(string filename, [CanBeNull] string previousResult)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            return Task.FromResult(MimeTypes.GetMimeType(filename));
        }
    }
}
