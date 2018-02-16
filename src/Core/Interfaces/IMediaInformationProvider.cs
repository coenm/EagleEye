namespace EagleEye.Core.Interfaces
{
    using System.Threading.Tasks;

    public interface IMediaInformationProvider
    {
        int Priority { get; }

        bool CanProvideInformation(string filename);

        Task ProvideAsync(string filename, MediaObject media);
    }
}