namespace EagleEye.ImageHash.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CoenM.ImageHash.HashAlgorithms;
    using Dawn;
    using EagleEye.Core.Data;
    using JetBrains.Annotations;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    internal static class ImageHashing
    {
        private static readonly PerceptualHash PerceptualHash;
        private static readonly DifferenceHash DifferenceHash;
        private static readonly AverageHash AverageHash;

        static ImageHashing()
        {
            PerceptualHash = new PerceptualHash();
            AverageHash = new AverageHash();
            DifferenceHash = new DifferenceHash();
        }

        /// <summary>
        /// Calculates three image hashes of an image.
        /// </summary>
        /// <param name="input">input stream of an image.</param>
        /// <remarks>Method does not dispose the <paramref name="input"/> stream neither will it 'reset' it's read position.</remarks>
        /// <returns>List of three hashes.</returns>
        /// <exception cref="InvalidDataException">Thrown when <paramref name="input"/> is not a valid image stream.</exception>
        [NotNull] public static List<PhotoHash> Calculate([NotNull] Stream input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            var result = new List<PhotoHash>(3);

            using (var image = LoadImageFromStream(input))
            {
                using (var clone = image.Clone())
                {
                    result.Add(
                        new PhotoHash
                        {
                            Hash = AverageHash.Hash(clone),
                            HashName = nameof(CoenM.ImageHash.HashAlgorithms.AverageHash),
                        });
                }

                using (var clone = image.Clone())
                {
                    result.Add(
                        new PhotoHash
                        {
                            Hash = DifferenceHash.Hash(clone),
                            HashName = nameof(CoenM.ImageHash.HashAlgorithms.DifferenceHash),
                        });
                }

                using (var clone = image.Clone())
                {
                    result.Add(
                        new PhotoHash
                        {
                            Hash = PerceptualHash.Hash(clone),
                            HashName = nameof(CoenM.ImageHash.HashAlgorithms.PerceptualHash),
                        });
                }
            }

            return result;
        }

        private static Image<Rgba32> LoadImageFromStream(Stream input)
        {
            try
            {
                return Image.Load(input);
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Could not read input stream as an image.", e);
            }
        }
    }
}
