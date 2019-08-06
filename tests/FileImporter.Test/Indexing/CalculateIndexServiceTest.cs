namespace EagleEye.FileImporter.Test.Indexing
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Json;
    using EagleEye.ImageHash.PhotoProvider;
    using EagleEye.TestHelper;
    using FluentAssertions;
    using Xunit;

    public class CalculateIndexServiceTest
    {
        private readonly string[] imageFileNames;

        public CalculateIndexServiceTest()
        {
            imageFileNames = Directory
                .GetFiles(TestImages.InputImagesDirectoryFullPath, "*.jpg", SearchOption.AllDirectories)
                .Select(ConvertToRelativeFilename)
                .ToArray();
        }

        [Fact]
        public void CalculateIndexOfFilesTest()
        {
            // arrange
            var expectedResult = JsonEncoding.Deserialize<List<ImageData>>(TestImagesIndex.IndexJson).Select(MapThis);
            var fileService = new RelativeFilesystemContentResolver(TestImages.InputImagesDirectoryFullPath);
            var sut = new CalculateIndexService(
                new FileSha256HashProvider(fileService),
                new ImageSharpPhotoHashProvider(fileService),
                new ImageSharpPhotoSha256HashProvider(fileService));

            // act
            var result = sut.CalculateIndex(imageFileNames);

            // assert
            var preparedResult = result.Select(MapThis);
            preparedResult.Should().BeEquivalentTo(expectedResult);
        }

        private static dynamic MapThis(ImageData data)
        {
            return new
                       {
                           MapedIdentifier = data.Identifier,
                           MapedAverageHash = data.Hashes.AverageHash,
                           MapedDifferenceHash = data.Hashes.DifferenceHash,
                           MapedFileHash = data.Hashes.FileHash,
                           MapedPerceptualHash = data.Hashes.PerceptualHash,
                       };
        }

        /// <summary>
        /// Convert fullFilename to relative such that it doesn't matter what machine in what directory the sln is stored.
        /// </summary>
        /// <param name="fullFilename">absolute filename.</param>
        /// <returns>filename relative to sn file.</returns>
        private static string ConvertToRelativeFilename(string fullFilename)
        {
            var slnDirectoryLength = TestImages.InputImagesDirectoryFullPath.Length;
            return fullFilename.Remove(0, slnDirectoryLength);
        }
    }
}
