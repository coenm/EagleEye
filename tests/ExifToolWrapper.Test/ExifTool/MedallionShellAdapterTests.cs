namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using FluentAssertions;

    using Xunit;
    using Xunit.Abstractions;

    [Xunit.Categories.IntegrationTest]
    public class MedallionShellAdapterTest : IDisposable
    {
        private const int FALLBACK_TEST_TIMEOUT = 5000;
        private readonly ITestOutputHelper _output;
        private readonly ExifToolStayOpenStream _stream;
        private readonly ManualResetEventSlim _mreSutExited;
        private readonly MedallionShellAdapter _sut;

        public MedallionShellAdapterTest(ITestOutputHelper output)
        {
            _output = output;
            _mreSutExited = new ManualResetEventSlim(false);
            var defaultArgs = new List<string>
                                    {
                                        ExifToolArguments.STAY_OPEN,
                                        ExifToolArguments.BOOL_TRUE,
                                        "-@",
                                        "-",
                                    };

            _stream = new ExifToolStayOpenStream(Encoding.UTF8);

            _sut = new MedallionShellAdapter(ExifToolSystemConfiguration.ExifToolExecutable, defaultArgs, _stream);
            _sut.ProcessExited += SutOnProcessExited;
        }

        public void Dispose()
        {
            _sut.ProcessExited -= SutOnProcessExited;
            _stream.Dispose();
        }

        [Fact]
        [Categories.ExifTool]
        public async Task KillingSutShouldInvokeProcessExitedEventTest()
        {
            // arrange

            // assume
            _sut.Finished.Should().BeFalse();

            // act
            _sut.Kill();
            await _sut.Task.ConfigureAwait(false);

            // assert
            AssertSutFinished(FALLBACK_TEST_TIMEOUT);
        }

        [Fact]
        [Categories.ExifTool]
        public async Task SettingStayOpenToFalseShouldCloseSutTest()
        {
            // arrange

            // assume
            _sut.Finished.Should().BeFalse();

            // act
            await _sut.WriteLineAsync(ExifToolArguments.STAY_OPEN).ConfigureAwait(false);
            await _sut.WriteLineAsync(ExifToolArguments.BOOL_FALSE).ConfigureAwait(false);

            _output.WriteLine("Awaiting task to finish");
            await _sut.Task.ConfigureAwait(false);
            _output.WriteLine("Task finished");

            // assert
            AssertSutFinished(FALLBACK_TEST_TIMEOUT);
        }

        private void AssertSutFinished(int timeout = 0)
        {
            _mreSutExited.Wait(timeout);
            _mreSutExited.IsSet.Should().BeTrue($"{nameof(_sut.ProcessExited)} event should have been fired.");
            _sut.Task.IsCompleted.Should().BeTrue("Task should have been completed.");
            _sut.Finished.Should().BeTrue($"{nameof(_sut.Finished)} property should be true.");
        }

        private void SutOnProcessExited(object sender, EventArgs eventArgs)
        {
            _mreSutExited.Set();
        }
    }
}