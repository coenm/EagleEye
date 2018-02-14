namespace EagleEye.FileImporter.Test.Indexing
{
    using System.IO;
    using System.Linq;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.FileImporter.Infrastructure.ContentResolver;
    using EagleEye.FileImporter.Json;
    using EagleEye.TestImages;

    using Xunit;

    public class CalculateIndexServiceTest
    {
        private readonly string[] _imageFilenames;

        public CalculateIndexServiceTest()
        {
            _imageFilenames = Directory
                .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "*.jpg", SearchOption.AllDirectories)
                .Select(ConvertToRelativeFilename)
                .ToArray();
        }

        // Convert Fullfilename to relative such that it doesn't matter what machine in what directory the sln is stored.
        private static string ConvertToRelativeFilename(string fullFilename)
        {
            var slnDirectoryLength = TestEnvironment.InputImagesDirectoryFullPath.Length;
            return fullFilename.Remove(0, slnDirectoryLength);
        }

        [Fact]
        public void CalculateIndexOfFilesTest()
        {
            // arrange
            var contentResolver = new RelativeFilesystemContentResolver(TestEnvironment.InputImagesDirectoryFullPath);
            var sut = new CalculateIndexService(contentResolver);

            // act
            var result = sut.CalculateIndex(_imageFilenames);

            // assert
            // take shortcut to assert the result.
            Assert.Equal(TestImagesIndex.IndexJson.Replace("\r\n", "\n"), JsonEncoding.Serialize(result.OrderBy(item => item.Identifier)).Replace("\r\n", "\n"));
        }
    }
}