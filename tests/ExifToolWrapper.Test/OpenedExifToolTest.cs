using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TestImages;
using Xunit;

namespace ExifToolWrapper.Test
{
    public class OpenedExifToolTest
    {
        private readonly string _image;
        private const string ExifToolExecutable = "exiftool.exe";

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
            using (var sut = new OpenedExifTool(ExifToolExecutable))
            {
                sut.Init();

                // act
                var task1 = sut.Execute(_image, new List<string>());
                var task2 = sut.Execute(_image, new List<string>());
                var task3 = sut.Execute(_image, new List<string>());

                // assert
                (await task3).Should().NotBeNullOrEmpty();
                (await task2).Should().NotBeNullOrEmpty();
                (await task1).Should().NotBeNullOrEmpty();

                sut.CancelPendingAndStop();
                //exifTool.Stop();
            }
        }
    }
}