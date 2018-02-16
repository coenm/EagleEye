namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;
    using EagleEye.TestImages;

    using FluentAssertions;

    using Xunit;

    public class OpenedExifToolTest
    {
        private const string EXIF_TOOL_EXECUTABLE = "exiftool.exe";
        private readonly string _image;

        public OpenedExifToolTest()
        {
            _image = Directory
                .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RunExifToolWithThreeCommands()
        {
            // arrange
            using (var sut = new OpenedExifTool(EXIF_TOOL_EXECUTABLE))
            {
                sut.Init();

                // act
                var task1 = sut.Execute(_image, new List<string>());
                var task2 = sut.Execute(_image, new List<string>());
                var task3 = sut.Execute(_image, new List<string>());

                // assert
                var result3 = await task3.ConfigureAwait(false);
                result3.Should().NotBeNullOrEmpty();

                var result2 = await task2.ConfigureAwait(false);
                result2.Should().NotBeNullOrEmpty();

                var result1 = await task1.ConfigureAwait(false);
                result1.Should().NotBeNullOrEmpty();

                sut.CancelPendingAndStop();
                //exifTool.Stop();
            }
        }
    }
}