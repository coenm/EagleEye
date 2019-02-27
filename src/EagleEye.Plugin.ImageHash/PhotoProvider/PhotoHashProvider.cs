namespace EagleEye.ImageHash.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class PhotoHashProvider : IPhotoHashProvider
    {
        public string Name => nameof(PhotoHashProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task<List<PhotoHash>> ProvideAsync(string filename, List<PhotoHash> previousResult)
        {
            DebugGuard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            throw new NotImplementedException();
        }
    }
}
