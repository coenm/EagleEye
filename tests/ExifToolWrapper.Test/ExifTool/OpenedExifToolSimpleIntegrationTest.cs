namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifToolSimplified;
    using EagleEye.TestImages;

    using FluentAssertions;

    using Xunit;
    using Xunit.Abstractions;

    public class OpenedExifToolSimpleIntegrationTest
    {
        private const int REPEAT = 100;
        private const string EXIF_TOOL_EXECUTABLE = "exiftool.exe";
        private readonly string _image;

        private readonly ITestOutputHelper _output;

        public OpenedExifToolSimpleIntegrationTest(ITestOutputHelper output)
        {
            _output = output;

            _image = Directory
                     .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                     .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        public async Task RunExiftoolForVersionAndImageTest()
        {
            // arrange
            var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE);
            sut.Init();

            // act
            var version = await sut.GetVersionAsync().ConfigureAwait(false);
            var result = await sut.ExecuteAsync(_image).ConfigureAwait(false);

            // assert
            version.Should().NotBeNullOrEmpty();
            result.Should().NotBeNullOrEmpty();

            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        [Xunit.Categories.Category("Performance")]
        public async Task RunWithInputStreamTest()
        {
            // arrange
            var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE);
            var sw = Stopwatch.StartNew();
            sut.Init();
            sw.Stop();
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to Initialize exiftool");

            // act
            sw.Reset();
            sw.Start();
            var version = string.Empty;
            for (var i = 0; i < REPEAT; i++)
                version = await sut.GetVersionAsync().ConfigureAwait(false);
            sw.Stop();
            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
            _output.WriteLine($"Version: {version}");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        public async Task InitAndDisposeTest()
        {
            // arrange
            var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE);

            // act
            sut.Init();
            await sut.DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token).ConfigureAwait(false);

            // assert
            //            sut.IsClosed.Should().Be(true);
        }
    }
}