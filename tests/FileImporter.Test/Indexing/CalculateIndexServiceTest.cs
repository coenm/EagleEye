namespace EagleEye.FileImporter.Test.Indexing
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.ImageHash.PhotoProvider;
    using EagleEye.TestHelper;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    public class CalculateIndexServiceTest : VerifyBase
    {
        private readonly string[] imageFileNames;

        public CalculateIndexServiceTest(ITestOutputHelper output)
            : base(output)
        {
            imageFileNames = Directory
                .GetFiles(TestImages.InputImagesDirectoryFullPath, "*.jpg", SearchOption.AllDirectories)
                // .Where(file => file.Contains("1") || file.Contains("2") || file.Contains("3"))
                .Where(file => !file.Contains("1") && !file.Contains("2") && !file.Contains("3"))
                .Select(ConvertToRelativeFilename)
                .ToArray();
        }

        [Fact]
        public Task CalculateIndexOfFilesTest()
        {
            // arrange
            var fileService = new RelativeSystemFileService(TestImages.InputImagesDirectoryFullPath);
            var sut = new CalculateIndexService(
                new FileSha256HashProvider(fileService),
                new ImageSharpPhotoHashProvider(fileService),
                new ImageSharpPhotoSha256HashProvider(fileService));

            // act
            var result = sut.CalculateIndex(imageFileNames);

            // assert
            return Verify(result.OrderBy(x => x.Identifier));
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
