namespace EagleEye.Core.Test.DefaultImplementations
{
    using System.IO;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.TestHelper;

    using FluentAssertions;

    using Xunit;

    public class SystemFileServiceTest
    {
        private readonly SystemFileService _sut;
        private readonly string _filename;

        public SystemFileServiceTest()
        {
            _sut = SystemFileService.Instance;
            _filename = Path.Combine(TestImages.InputImagesDirectoryFullPath, "1.jpg");
        }

        [Fact]
        public void FileExistsTest()
        {
            // arrange

            // act
            var result = _sut.FileExists(_filename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OpenReadTest()
        {
            // arrange
            var filename = Path.Combine(_filename);

            // act
            using (var result = _sut.OpenRead(filename))
            {
                // assert
                result.Should().NotBeNull();
            }
        }
    }
}