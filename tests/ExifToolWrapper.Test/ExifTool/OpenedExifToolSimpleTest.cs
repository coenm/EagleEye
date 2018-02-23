namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifToolSimplified;
    using EagleEye.TestImages;

    using FluentAssertions;

    using Xunit;
    using Xunit.Abstractions;

    public class OpenedExifToolSimpleTest
    {
        private const int REPEAT = 100;
        private const string EXIF_TOOL_EXECUTABLE = "exiftool.exe";
        private readonly string _image;

        private readonly ITestOutputHelper _output;

        public OpenedExifToolSimpleTest(ITestOutputHelper output)
        {
            _output = output;

            _image = Directory
                .GetFiles(TestEnvironment.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                .SingleOrDefault();

            _image.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RunExifToolWithThreeCommands()
        {
            // arrange
            using (var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE))
            {
                sut.Init();

                // act
                var version = await sut.GetVersionAsync().ConfigureAwait(false);
                var result = await sut.ExecuteAsync(_image).ConfigureAwait(false);

                // assert
                version.Should().NotBeNullOrEmpty();
                result.Should().NotBeNullOrEmpty();

                await sut.DisposeAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task RunWithInputStreamTest()
        {
            // arrange
            using (var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE))
            {
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

                // assert
                _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
                _output.WriteLine($"Version: {version}");
                version.Should().NotBeNullOrEmpty();

                await sut.DisposeAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public void RunWithoutInputStreamTest()
        {
            // arrange
            var sut = new ClosedExifToolSimple(EXIF_TOOL_EXECUTABLE);

            // act
            var sw = Stopwatch.StartNew();
            var version = string.Empty;
            for (int i = 0; i < REPEAT; i++)
                version = sut.Execute(new object[] { "-ver" });
            sw.Stop();

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void OpenDisposeTest()
        {
            using (var sut = new OpenedExifToolSimple(EXIF_TOOL_EXECUTABLE))
            {
                sut.Init();
            }
        }
    }
}