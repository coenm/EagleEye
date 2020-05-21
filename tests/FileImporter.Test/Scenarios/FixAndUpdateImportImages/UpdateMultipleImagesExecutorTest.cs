namespace EagleEye.FileImporter.Test.Scenarios.FixAndUpdateImportImages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Scenarios.FixAndUpdateImportImages;
    using EagleEye.FileImporter.Similarity;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class UpdateMultipleImagesExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_ShouldNotWriteToProgress_WhenNoFilesToHandle()
        {
            // arrange
            var updateImportImageCommandHandler = A.Dummy<IUpdateImportImageCommandHandler>();
            var progress = A.Fake<IProgress<FileProcessingProgress>>();
            var sut = new UpdateMultipleImagesExecutor(updateImportImageCommandHandler);

            // act
            await sut.ExecuteAsync(Enumerable.Empty<string>(), progress, CancellationToken.None);

            // assert
            A.CallTo(progress).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldProcessSingleFile_WhenFileIsGiven()
        {
            // arrange
            var updateImportImageCommandHandler = A.Fake<IUpdateImportImageCommandHandler>();
            var progress = A.Fake<IProgress<FileProcessingProgress>>();
            var sut = new UpdateMultipleImagesExecutor(updateImportImageCommandHandler);
            var files = new[] { "file1", };
            var progressReport = new List<FileProcessingProgress>();
            A.CallTo(() => progress.Report(A<FileProcessingProgress>._))
             .Invokes(call =>
                      {
                          var fileProcessingProgress = (FileProcessingProgress)call.Arguments.First();
                          progressReport.Add(fileProcessingProgress);
                      });

            // act
            await sut.ExecuteAsync(files, progress, CancellationToken.None);

            // assert
            // calls happened and the order or calls
            A.CallTo(() => progress.Report(A<FileProcessingProgress>._)).MustHaveHappened()
             .Then(A.CallTo(() => updateImportImageCommandHandler.HandleAsync("file1", A<CancellationToken>._)).MustHaveHappened())
             .Then(A.CallTo(() => progress.Report(A<FileProcessingProgress>._)).MustHaveHappened());

            // assert data
            var expectedProgressReports = new[]
                                          {
                                              new FileProcessingProgress("file1", 1, 2, "Start", ProgressState.Busy),
                                              new FileProcessingProgress("file1", 2, 2, "Finished", ProgressState.Success),
                                          };
            progressReport.Should().BeEquivalentTo(expectedProgressReports);
        }
    }
}
