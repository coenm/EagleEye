namespace EagleEye.Picasa.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;
    using EagleEye.Picasa.Picasa;

    using FakeItEasy;

    using FluentAssertions;

    using JetBrains.Annotations;

    using Xunit;

    public class PicasaServiceTest : IDisposable
    {
        private readonly string _imageFilename;
        private readonly string _picasaFilename;
        private readonly string _tempPath;
        private readonly TestablePicasaService _sut;
        private readonly IFileService _fileService;
        private readonly Stream _picasaIniStream;

        public PicasaServiceTest()
        {
            _tempPath = Path.GetTempPath();
            _imageFilename = GetFilename("image.jpg");
            _picasaFilename = GetFilename(".picasa.ini");

            _fileService = A.Fake<IFileService>();
            _sut = new TestablePicasaService(_fileService);

            _picasaIniStream = new MemoryStream();
            A.CallTo(() => _fileService.FileExists(_imageFilename)).Returns(true);
            A.CallTo(() => _fileService.FileExists(_picasaFilename)).Returns(true);
            A.CallTo(() => _fileService.OpenRead(_imageFilename)).Throws(new Exception("Should not be called"));
            A.CallTo(() => _fileService.OpenRead(_picasaFilename)).Returns(_picasaIniStream);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void CanProvideDataChecksImageAndPicasaFileExistanceTest(bool imageExists, bool picasaFileExists, bool expectedResult)
        {
            // arrange
            A.CallTo(() => _fileService.FileExists(_imageFilename)).Returns(imageExists);
            A.CallTo(() => _fileService.FileExists(_picasaFilename)).Returns(picasaFileExists);

            // acts
            var result = _sut.CanProvideData(_imageFilename);

            // assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetDataShouldUsePicasaIniFileToGetPersonDataTest()
        {
            // arrange
            var expectedRestult = new FileWithPersons("image.jpg", "Stephen Hawking", "Nelson Mandela");
            var fileWithPersonsList = new[]
                                          {
                                              new FileWithPersons("imageA.jpg", "Alice", "Bob"),
                                              new FileWithPersons("imageC.jpg", "Stephen Hawking", "Alice", "Bob"),
                                              expectedRestult,
                                          };
            _sut.SetGetFileAndPersonDataImplementation(stream =>
                                                       {
                                                           if (stream != null && stream == _picasaIniStream)
                                                               return fileWithPersonsList;
                                                           return null;
                                                       });

            // act
            var result = await _sut.GetDataAsync(GetFilename("image.jpg")).ConfigureAwait(false);

            // assert
            result.Should().Be(expectedRestult);
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
            _sut.SetGetFileAndPersonDataImplementation(_ =>
                                                       {
                                                           methodInvokedCounter++;
                                                           mreSimulateTaskDuration.Wait();
                                                           return new[] { dataImageA, dataImageB, dataImageC };
                                                       });

            // act
            var resultTask1 = _sut.GetDataAsync(GetFilename("image.jpg"));
            var resultTask2 = _sut.GetDataAsync(GetFilename("imageC.jpg"));
            var resultTask3 = _sut.GetDataAsync(GetFilename("image.jpg"));

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
            _sut?.Dispose();
        }

        private string GetFilename(string filename)
        {
            return Path.Combine(_tempPath, "photos", filename);
        }

        private class TestablePicasaService : PicasaService
        {
            private Func<Stream, IEnumerable<FileWithPersons>> _getFileAndPersonFunc;

            public TestablePicasaService([NotNull] IFileService fileService)
                : base(fileService)
            {
            }

            public void SetGetFileAndPersonDataImplementation(Func<Stream, IEnumerable<FileWithPersons>> getFileAndPersonFunc = null)
            {
                _getFileAndPersonFunc = getFileAndPersonFunc;
            }

            protected override IEnumerable<FileWithPersons> GetFileAndPersonData(Stream stream)
            {
                if (_getFileAndPersonFunc != null)
                    return _getFileAndPersonFunc.Invoke(stream);
                return base.GetFileAndPersonData(stream);
            }
        }
    }
}