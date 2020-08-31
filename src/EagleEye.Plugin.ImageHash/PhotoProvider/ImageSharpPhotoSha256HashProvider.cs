namespace EagleEye.ImageHash.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [UsedImplicitly]
    public class ImageSharpPhotoSha256HashProvider : IPhotoSha256HashProvider
    {
        [NotNull]
        private readonly IFileService fileService;

        public ImageSharpPhotoSha256HashProvider([NotNull] IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public string Name => nameof(ImageSharpPhotoSha256HashProvider);

        public uint Priority => 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task<ReadOnlyMemory<byte>> ProvideAsync(string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            try
            {
                var result = Provide(filename);
                return Task.FromResult(result);
            }
            catch (Exception)
            {
                return Task.FromResult<ReadOnlyMemory<byte>>(null);
            }
        }

        private static byte[] CalculateImageHash([NotNull] Image<Rgba32> img)
        {
            Guard.Argument(img, nameof(img)).NotNull();

            byte[] data = Array.Empty<byte>();

            if (img.TryGetSinglePixelSpan(out var fullImagePixelSpan))
            {
                data = MemoryMarshal.AsBytes(fullImagePixelSpan).ToArray();
            }
            else
            {
                // stupid implementation
                var listOfByteArrays = new List<byte[]>(img.Height);

                for (int row = 0; row < img.Height; row++)
                    listOfByteArrays.Add(MemoryMarshal.AsBytes(img.GetPixelRowSpan(row)).ToArray());

                data = listOfByteArrays.SelectMany(x => x).ToArray();
            }

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(data);
        }

        private ReadOnlyMemory<byte> Provide(string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            using var stream = fileService.OpenRead(filename);
            using var image = Image.Load<Rgba32>(stream);
            image.Mutate(x => x.AutoOrient());
            var result = CalculateImageHash(image);
            return new ReadOnlyMemory<byte>(result);
        }
    }
}
