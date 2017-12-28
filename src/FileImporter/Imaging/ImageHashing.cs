using System.IO;
using System.Security.Cryptography;
using CoenM.ImageSharp;
using CoenM.ImageSharp.HashAlgorithms;
using SixLabors.ImageSharp;

namespace FileImporter.Imaging
{
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
                FileHash = CalculateStreamHash(input)
            };

            input.Position = 0;

            using (var image = Image.Load(input))
            {
                result.ImageHash = CalculateImageHash(image);

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

        
        private static byte[] CalculateStreamHash(Stream input)
        {
            using (var sha256 = SHA256.Create())
                return sha256.ComputeHash(input);
        }

        private static byte[] CalculateImageHash(Image<Rgba32> img)
        {
            var data = img.SavePixelData();

            using (var sha256 = SHA256.Create())
                return sha256.ComputeHash(data);
        }
    } 
}