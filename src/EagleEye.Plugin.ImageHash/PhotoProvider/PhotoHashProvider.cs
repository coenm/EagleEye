namespace EagleEye.ImageHash.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ImageHash.Internal;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class PhotoHashProvider : IPhotoHashProvider
    {
        [NotNull]
        private readonly IFileService fileService;

        public PhotoHashProvider([NotNull] IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public string Name => nameof(PhotoHashProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task<List<PhotoHash>> ProvideAsync(string filename)
        {
            try
            {
                using (var stream = fileService.OpenRead(filename))
                {
                    return Task.FromResult(ImageHashing.Calculate(stream));
                }
            }
            catch (Exception)
            {
                return Task.FromResult(null as List<PhotoHash>);
            }
        }
    }
}
