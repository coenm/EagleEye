namespace EagleEye.FileImporter.Imaging
{
    using System.IO;

    using CoenM.ImageHash;
    using CoenM.ImageHash.HashAlgorithms;
    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.ImageHash.PhotoProvider;
    using SixLabors.ImageSharp;

    public static class ImageHashing
    {
        private static readonly IImageHash PHash;
        private static readonly DifferenceHash DHash;
        private static readonly AverageHash AHash;

        static ImageHashing()
        {
            PHash = new PerceptualHash();
            AHash = new AverageHash();
            DHash = new DifferenceHash();
        }

        public static ImageHashValues Calculate(Stream input)
        {
            var result = new ImageHashValues
            {
                FileHash = FileSha256HashProvider.CalculateStreamHash(input),
            };

            input.Position = 0;

            using (var image = Image.Load(input))
            {
                using (var clone = image.Clone())
                    result.ImageHash = ImageSharpPhotoSha256HashProvider.CalculateImageHash(clone);

                using (var clone = image.Clone())
                    result.AverageHash = AHash.Hash(clone);

                using (var clone = image.Clone())
                    result.DifferenceHash = DHash.Hash(clone);

                using (var clone = image.Clone())
                    result.PerceptualHash = PHash.Hash(clone);
            }

            input.Position = 0;

            return result;
        }
    }
}
