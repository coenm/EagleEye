namespace EagleEye.Core.Test.DefaultImplementations
{
    using System.IO;

    using EagleEye.Core.DefaultImplementations;
    using EagleEye.TestHelper;
    using FluentAssertions;
    using Xunit;

    public class SystemFileServiceTest
    {
        private readonly SystemFileService sut;
        private readonly string filename;

        public SystemFileServiceTest()
        {
            sut = SystemFileService.Instance;
            filename = Path.Combine(TestImages.InputImagesDirectoryFullPath, "1.jpg");
        }

        [Fact]
        public void FileExistsTest()
        {
            // arrange

            // act
            var result = sut.FileExists(filename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void OpenReadTest()
        {
            // arrange
            var fileName = Path.Combine(filename);

            // act
            using (var result = sut.OpenRead(fileName))
            {
                // assert
                result.Should().NotBeNull();
            }
        }
    }
}
