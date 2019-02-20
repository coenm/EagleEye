namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using JetBrains.Annotations;

    public interface IPhotoDateTimeTakenProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<Timestamp> ProvideAsync([NotNull] string filename, [CanBeNull] Timestamp previousResult);
    }
}
