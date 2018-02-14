namespace EagleEye.FileImporter.Test.Infrastructure.Everything
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.TestImages;

    using Xunit;

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