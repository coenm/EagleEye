namespace EagleEye.Picasa.Test.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Picasa.PhotoProvider;
    using EagleEye.Picasa.Picasa;

    using FakeItEasy;

    using FluentAssertions;

    using JetBrains.Annotations;

    using Xunit;

    public class PicasaServiceTest : IDisposable
    {
        private readonly string imageFilename;
        private readonly string picasaFilename;
        private readonly string tempPath;
        private readonly TestablePicasaService sut;
        private readonly IFileService fileService;
        private readonly Stream picasaIniStream;

        public PicasaServiceTest()
        {
            tempPath = Path.GetTempPath();
            imageFilename = GetFilename("image.jpg");
            picasaFilename = GetFilename(".picasa.ini");

            fileService = A.Fake<IFileService>();
            sut = new TestablePicasaService(fileService);

            picasaIniStream = new MemoryStream();
            A.CallTo(() => fileService.FileExists(imageFilename)).Returns(true);
            A.CallTo(() => fileService.FileExists(picasaFilename)).Returns(true);
            A.CallTo(() => fileService.OpenRead(imageFilename)).Throws(new Exception("Should not be called"));
            A.CallTo(() => fileService.OpenRead(picasaFilename)).Returns(picasaIniStream);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void CanProvideDataChecksImageAndPicasaFileExistenceTest(bool imageExists, bool picasaFileExists, bool expectedResult)
        {
            // arrange
            A.CallTo(() => fileService.FileExists(imageFilename)).Returns(imageExists);
            A.CallTo(() => fileService.FileExists(picasaFilename)).Returns(picasaFileExists);

            // acts
            var result = sut.CanProvideData(imageFilename);

            // assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetDataShouldUsePicasaIniFileToGetPersonDataTest()
        {
            // arrange
            var expectedResult = new FileWithPersons("image.jpg", "Stephen Hawking", "Nelson Mandela");
            var fileWithPersonsList = new[]
                                          {
                                              new FileWithPersons("imageA.jpg", "Alice", "Bob"),
                                              new FileWithPersons("imageC.jpg", "Stephen Hawking", "Alice", "Bob"),
                                              expectedResult,
                                          };
            sut.SetGetFileAndPersonDataImplementation(stream =>
                                                       {
                                                           if (stream != null && stream == picasaIniStream)
                                                               return fileWithPersonsList;
                                                           return null;
                                                       });

            // act
            var result = await sut.GetDataAsync(GetFilename("image.jpg")).ConfigureAwait(false);

            // assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetDataShouldCachePicasaParsingTasksTest()
        {
            // arrange
            var mreSimulateTaskDuration = new ManualResetEventSlim(false);
            var methodInvokedCounter = 0;

            var dataImageA = new FileWithPersons("imageA.jpg", "Alice", "Bob");
            var dataImageB = new FileWithPersons("image.jpg", "Stephen Hawking", "Nelson Mandela");
            var dataImageC = new FileWithPersons("imageC.jpg", "Stephen Hawking", "Alice", "Bob");
            sut.SetGetFileAndPersonDataImplementation(_ =>
                                                       {
                                                           methodInvokedCounter++;
                                                           mreSimulateTaskDuration.Wait();
                                                           return new[] { dataImageA, dataImageB, dataImageC };
                                                       });

            // act
            var resultTask1 = sut.GetDataAsync(GetFilename("image.jpg"));
            var resultTask2 = sut.GetDataAsync(GetFilename("imageC.jpg"));
            var resultTask3 = sut.GetDataAsync(GetFilename("image.jpg"));

            mreSimulateTaskDuration.Set();

            var result1 = await resultTask1.ConfigureAwait(false);
            var result2 = await resultTask2.ConfigureAwait(false);
            var result3 = await resultTask3.ConfigureAwait(false);

            // assert
            result1.Should().Be(dataImageB);
            result2.Should().Be(dataImageC);
            result3.Should().Be(dataImageB);
            methodInvokedCounter.Should().Be(1);
        }

        public void Dispose()
        {
            sut?.Dispose();
        }

        private string GetFilename(string filename)
        {
            return Path.Combine(tempPath, "photos", filename);
        }

        private class TestablePicasaService : PicasaService
        {
            private Func<Stream, IEnumerable<FileWithPersons>> getFileAndPersonFunc;

            public TestablePicasaService([NotNull] IFileService fileService)
                : base(fileService)
            {
            }

            public void SetGetFileAndPersonDataImplementation(Func<Stream, IEnumerable<FileWithPersons>> func = null)
            {
                getFileAndPersonFunc = func;
            }

            protected override IEnumerable<FileWithPersons> GetFileAndPersonData(Stream stream)
            {
                if (getFileAndPersonFunc != null)
                    return getFileAndPersonFunc.Invoke(stream);
                return base.GetFileAndPersonData(stream);
            }
        }
    }
}
