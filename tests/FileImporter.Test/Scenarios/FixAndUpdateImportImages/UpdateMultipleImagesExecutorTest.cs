namespace EagleEye.FileImporter.Test.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages;
    using EagleEye.FileImporter.Similarity;
    using FakeItEasy;
    using Xunit;

    public class UpdateMultipleImagesExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldNotWriteToProgress_WhenNoFilesToHandle()
        {
            // arrange
            var updateImportImageCommandHandler = A.Dummy<IUpdateImportImageCommandHandler>();
            var sut = new UpdateMultipleImagesExecutor(updateImportImageCommandHandler);

            // act
            IProgress<FileProcessingProgress> progress = A.Fake<IProgress<FileProcessingProgress>>();
            await sut.ExecuteAsync(Enumerable.Empty<string>(), progress, CancellationToken.None);

            // assert
            A.CallTo(progress).MustNotHaveHappened();
        }
    }
}
