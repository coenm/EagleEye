namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IPhotoMimeTypeProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<string> ProvideAsync([NotNull] string filename, [CanBeNull] string previousResult);
    }
}
