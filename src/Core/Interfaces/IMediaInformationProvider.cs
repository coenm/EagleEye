namespace EagleEye.Core.Interfaces
{
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IMediaInformationProvider
    {
        int Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task ProvideAsync([NotNull] string filename, [NotNull] MediaObject media);
    }
}