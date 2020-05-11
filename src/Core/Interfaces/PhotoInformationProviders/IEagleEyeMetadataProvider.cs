namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.EagleEyeXmp;
    using JetBrains.Annotations;

    public interface IEagleEyeMetadataProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<EagleEyeMetadata> ProvideAsync([NotNull] string filename, CancellationToken ct = default);
    }
}
