namespace EagleEye.FileImporter.Test.Infrastructure.Everything
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Indexing;
    using EagleEye.TestHelper;

    using Xunit;

    public class EverythingTest
    {
        private readonly string[] imageFilenames;

        public EverythingTest()
        {
            imageFilenames = Directory.GetFiles(TestImages.InputImagesDirectoryFullPath, "*.jpg", SearchOption.AllDirectories).ToArray();
        }

        [Fact(Skip = "Requires Everything.exe")]
        public async Task ManualTestIfEverythingIsStartedTest()
        {
            // arrange
            var sut = new FileImporter.Infrastructure.Everything.Everything();
            var fileIndexes = imageFilenames.Select(p => new ImageData(p)).ToList();

            // act
            await sut.Show(fileIndexes);

            // assert
        }
    }
}