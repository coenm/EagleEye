namespace EagleEye.Core.Interfaces
{
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IMediaInformationProvider
    {
        int Priority { get; }

        bool CanProvideInformation(string filename);

        Task ProvideAsync(string filename, [NotNull] MediaObject media);
    }
}