using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileImporter.Indexing;
using TestImages;
using Xunit;

namespace FileImporter.Test.Infrastructure.Everything
{
    public class EverythingTest
    {
        private readonly string[] _imageFilenames;

        public EverythingTest()
        {
            _imageFilenames = Directory.GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "*.jpg", SearchOption.AllDirectories).ToArray();
        }
        
        [Fact(Skip = "Requires Everything.exe")]
        public async Task ManualTestIfEverythingIsStartedTest()
        {
            // arrange
            var sut = new FileImporter.Infrastructure.Everything.Everything();
            var fileIndexes = _imageFilenames.Select(p => new ImageData(p)).ToList();

            // act
            await sut.Show(fileIndexes);

            // assert
        }
    }
}