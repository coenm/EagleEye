using System.IO;
using System.Linq;
using FileImporter.Indexing;
using FileImporter.Infrastructure.ContentResolver;
using FileImporter.Json;
using TestImages;
using Xunit;

namespace FileImporter.Test.Indexing
{
    public class IndexDirectoryServiceTest
    {
        private readonly string[] _imageFilenames;

        public IndexDirectoryServiceTest()
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
            var result = fullFilename.Remove(0, slnDirectoryLength);
            return result;
        }


        [Fact]
        public void CalculateIndexOfFilesTest()
        {
            // arrange
            var contentResolver = new RelativeFilesystemContentResolver(TestEnvironment.InputImagesDirectoryFullPath);
            var sut = new IndexDirectoryService(contentResolver);

            // act
            var result = sut.CalculateIndex(_imageFilenames);

            // assert
            // take shortcut to assert the result.
            Assert.Equal(TestImagesIndex.IndexJson, JsonEncoding.Serialize(result.OrderBy(item => item.Identifier)));
        }
    }
}