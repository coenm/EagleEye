namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using JetBrains.Annotations;

    public interface IMediaInformationProvider
    {
        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task ProvideAsync([NotNull] string filename, [NotNull] MediaObject media);
    }
}
