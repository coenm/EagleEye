namespace EagleEye.ImageHash.PhotoProvider
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ImageHash.Internal;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class PhotoHashProvider : IPhotoHashProvider
    {
        [NotNull]
        private readonly IFileService fileService;

        public PhotoHashProvider([NotNull] IFileService fileService)
        {
            Dawn.Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public string Name => nameof(PhotoHashProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task<List<PhotoHash>> ProvideAsync(string filename, List<PhotoHash> previousResult)
        {
            Helpers.Guards.Guard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            using (var stream = fileService.OpenRead(filename))
            {
                return Task.FromResult(ImageHashing.Calculate(stream));
            }
        }
    }
}
