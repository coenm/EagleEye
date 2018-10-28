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
        private const int FallbackTestTimeout = 5000;
        private readonly ITestOutputHelper output;
        private readonly ExifToolStayOpenStream stream;
        private readonly ManualResetEventSlim mreSutExited;
        private readonly MedallionShellAdapter sut;

        public MedallionShellAdapterTest(ITestOutputHelper output)
        {
            this.output = output;
            mreSutExited = new ManualResetEventSlim(false);
            var defaultArgs = new List<string>
                                    {
                                        ExifToolArguments.STAY_OPEN,
                                        ExifToolArguments.BOOL_TRUE,
                                        "-@",
                                        "-",
                                    };

            stream = new ExifToolStayOpenStream(Encoding.UTF8);

            sut = new MedallionShellAdapter(ExifToolSystemConfiguration.ExifToolExecutable, defaultArgs, stream);
            sut.ProcessExited += SutOnProcessExited;
        }

        public void Dispose()
        {
            sut.ProcessExited -= SutOnProcessExited;
            stream.Dispose();
        }

        [Fact]
        [Categories.ExifTool]
        public async Task KillingSutShouldInvokeProcessExitedEventTest()
        {
            // arrange

            // assume
            sut.Finished.Should().BeFalse();

            // act
            sut.Kill();
            await sut.Task.ConfigureAwait(false);

            // assert
            AssertSutFinished(FallbackTestTimeout);
        }

        [Fact]
        [Categories.ExifTool]
        public async Task SettingStayOpenToFalseShouldCloseSutTest()
        {
            // arrange

            // assume
            sut.Finished.Should().BeFalse();

            // act
            await sut.WriteLineAsync(ExifToolArguments.STAY_OPEN).ConfigureAwait(false);
            await sut.WriteLineAsync(ExifToolArguments.BOOL_FALSE).ConfigureAwait(false);

            output.WriteLine("Awaiting task to finish");
            await sut.Task.ConfigureAwait(false);
            output.WriteLine("Task finished");

            // assert
            AssertSutFinished(FallbackTestTimeout);
        }

        private void AssertSutFinished(int timeout = 0)
        {
            mreSutExited.Wait(timeout);
            mreSutExited.IsSet.Should().BeTrue($"{nameof(sut.ProcessExited)} event should have been fired.");
            sut.Task.IsCompleted.Should().BeTrue("Task should have been completed.");
            sut.Finished.Should().BeTrue($"{nameof(sut.Finished)} property should be true.");
        }

        private void SutOnProcessExited(object sender, EventArgs eventArgs)
        {
            mreSutExited.Set();
        }
    }
}
