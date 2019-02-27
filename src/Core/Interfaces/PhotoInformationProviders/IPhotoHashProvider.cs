namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using JetBrains.Annotations;

    public interface IPhotoHashProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<List<PhotoHash>> ProvideAsync([NotNull] string filename, [CanBeNull] List<PhotoHash> previousResult);
    }
}
