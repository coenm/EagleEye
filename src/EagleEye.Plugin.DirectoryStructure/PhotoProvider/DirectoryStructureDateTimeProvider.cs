namespace EagleEye.DirectoryStructure.PhotoProvider
{
    using System;
    using System.IO;
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

            filename = filename.Trim();

            try
            {
                filename = Path.GetFileName(filename);
            }
            catch (Exception)
            {
                return false;
            }

            // todo coenm. improve implementation ;-)
            if (filename.StartsWith("2000"))
                return true;

            return false;
        }

        public Task<Timestamp> ProvideAsync(string filename, Timestamp previousResult)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            filename = filename.Trim();

            try
            {
                filename = Path.GetFileName(filename);
            }
            catch (Exception)
            {
                return Task.FromResult<Timestamp>(null);
            }

            // todo coenm. improve implementation ;-)
            if (filename.StartsWith("2000"))
                return Task.FromResult(new Timestamp(2000));

            return Task.FromResult<Timestamp>(null);
        }
    }
}
