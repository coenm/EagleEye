namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using JetBrains.Annotations;

    public interface IPhotoLocationProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<Location> ProvideAsync([NotNull] string filename, [CanBeNull] Location previousResult);
    }
}
