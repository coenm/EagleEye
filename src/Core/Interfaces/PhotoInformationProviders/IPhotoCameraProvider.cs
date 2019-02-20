namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using JetBrains.Annotations;

    public interface IPhotoCameraProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<CameraInformation> ProvideAsync([NotNull] string filename, [CanBeNull] CameraInformation previousResult);
    }
}
