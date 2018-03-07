namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using FluentAssertions;

    using Xunit;
    using Xunit.Abstractions;

    public class ClosedExifToolTest
    {
        private const int REPEAT = 100;
        private readonly ITestOutputHelper _output;
        private CancellationTokenSource _cts;

        public ClosedExifToolTest(ITestOutputHelper output)
        {
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            _output = output;
        }

        [Fact(Skip = "Does not work on AppVeyor")]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        [Xunit.Categories.Category("Performance")]
        public async Task RunWithoutInputStreamTest()
        {
            // arrange
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(60 * REPEAT));
            var sut = new ClosedExifTool(ExifToolSystemConfiguration.ExifToolExecutable);

            // act
            var version = string.Empty;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < REPEAT; i++)
            {
                version = await sut.ExecuteAsync(new[] { "-ver" }, _cts.Token).ConfigureAwait(false);
            }

            sw.Stop();

            // assert
            _output.WriteLine($"It took {sw.Elapsed.ToString()} to retrieve exiftool version {REPEAT} times");
            version.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        public async Task ExecuteAsyncToGetVersionTest()
        {
            // arrange
            var sut = new ClosedExifTool(ExifToolSystemConfiguration.ExifToolExecutable);

            // act
            var sw = Stopwatch.StartNew();
            var result = await sut.ExecuteAsync(new[] { "-ver" }, _cts.Token).ConfigureAwait(false);
            sw.Stop();

            // assert
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Xunit.Categories.IntegrationTest]
        [Xunit.Categories.Category("ExifTool")]
        public void ExecuteWithUnknownFileShouldThrowTest()
        {
            // arrange
            var sut = new ClosedExifTool(ExifToolSystemConfiguration.ExifToolExecutable);

            // act
            Func<Task> act = () => sut.ExecuteAsync(new[] { "fake" }, _cts.Token);

            // assert
            act.Should().Throw<ExiftoolException>().WithMessage("File not found: fake" + Environment.NewLine);
        }


        [Fact]
        [Xunit.Categories.IntegrationTest]
        public void ExecuteWithUnknownExecutableFileShouldThrowTest()
        {
            // arrange
            var sut = new ClosedExifTool(ExifToolSystemConfiguration.ExifToolExecutable + "fake");

            // act
            Func<Task> act = () => sut.ExecuteAsync(new[] { "-ver" }, _cts.Token);

            // assert
            act.Should().Throw<System.ComponentModel.Win32Exception>();
        }
    }
}