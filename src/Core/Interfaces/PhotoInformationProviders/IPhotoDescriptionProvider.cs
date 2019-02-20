namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IPhotoDescriptionProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<List<string>> ProvideAsync([NotNull] string filename);
    }
}