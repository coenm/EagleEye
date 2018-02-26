namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System.Diagnostics;

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
        public void RunWithoutInputStreamTest()
        {
            // arrange
            var sut = new ClosedExifToolSimple(EXIF_TOOL_EXECUTABLE);

            // act
            var sw = Stopwatch.StartNew();
            var version = string.Empty;
            for (var i = 0; i < REPEAT; i++)
                version = sut.Execute(new object[] { "-ver" });
            sw.Stop();

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
            version.Should().NotBeNullOrEmpty();
        }
    }
}