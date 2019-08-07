namespace EagleEye.Core.Interfaces.PhotoInformationProviders
{
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    public interface IFileSha256HashProvider
    {
        string Name { get; }

        uint Priority { get; }

        bool CanProvideInformation([NotNull] string filename);

        Task<ReadOnlyMemory<byte>> ProvideAsync([NotNull] string filename);
    }
}
