namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;
    using EagleEye.ExifToolWrapper.ExifToolSimplified;

    using FluentAssertions;

    using Xunit;
    using Xunit.Abstractions;

    public class ClosedExifToolSimpleTest
    {
        private const int REPEAT = 100;
        private const string EXIF_TOOL_EXECUTABLE = "exiftool.exe";

        private readonly ITestOutputHelper _output;

        public ClosedExifToolSimpleTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        [Xunit.Categories.Category("Performance")]
        public async Task RunWithoutInputStreamTest()
        {
            // arrange
            var sut = new ClosedExifToolSimple(EXIF_TOOL_EXECUTABLE);

            // act
            var sw = Stopwatch.StartNew();
            var version = string.Empty;
            for (var i = 0; i < REPEAT; i++)
                version = await sut.ExecuteAsync(new object[] { "-ver" }).ConfigureAwait(false);
            sw.Stop();

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        public void ExecuteWithUnknownFileShouldThrowTest()
        {
            // arrange
            var sut = new ClosedExifToolSimple(EXIF_TOOL_EXECUTABLE);

            // act
            Func<Task> act = () => sut.ExecuteAsync(new object[] { "fake" });

            // assert
            act.Should().Throw<ExiftoolException>().WithMessage("File not found: fake" + Environment.NewLine);
        }


        [Fact]
        [Xunit.Categories.IntegrationTest]
        public void ExecuteWithUnknownExecutableFileShouldThrowTest()
        {
            // arrange
            var sut = new ClosedExifToolSimple(EXIF_TOOL_EXECUTABLE + "fake");

            // act
            Func<Task> act = () => sut.ExecuteAsync(new object[] { "-ver" });

            // assert
            act.Should().Throw<System.ComponentModel.Win32Exception>().WithMessage("The system cannot find the file specified");
        }
    }
}