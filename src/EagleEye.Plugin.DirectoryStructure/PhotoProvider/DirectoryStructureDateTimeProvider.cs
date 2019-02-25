namespace EagleEye.DirectoryStructure.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class DirectoryStructureDateTimeProvider : IPhotoDateTimeTakenProvider
    {
        public string Name => nameof(DirectoryStructureDateTimeProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            // todo coenm
            return false;
        }

        public Task<Timestamp> ProvideAsync(string filename, Timestamp previousResult)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            throw new System.NotImplementedException();
        }
    }
}
