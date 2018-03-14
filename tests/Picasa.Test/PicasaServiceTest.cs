namespace EagleEye.Picasa.Test
{
    using System;
    using System.IO;

    using EagleEye.Core.Interfaces;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    public class PicasaServiceTest : IDisposable
    {
        private readonly string _imageFilename;
        private readonly string _picasaFilename;
        private readonly PicasaService _sut;
        private readonly IFileService _fileService;

        public PicasaServiceTest()
        {
            var tempPath = Path.GetTempPath();
            _imageFilename = Path.Combine(tempPath, "photos", "image.jpg");
            _picasaFilename = Path.Combine(tempPath, "photos", ".picasa.ini");

            _fileService = A.Fake<IFileService>();
            _sut = new PicasaService(_fileService);
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

        public void Dispose()
        {
            _sut?.Dispose();
        }
    }
}