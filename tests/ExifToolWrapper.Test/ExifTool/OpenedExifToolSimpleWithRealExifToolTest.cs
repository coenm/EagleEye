namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    public class OpenedExifToolSimpleTest
    {
        private readonly OpenedExifTool _sut;
        private readonly IMedallionShell _mediallionShell;
        private List<string> _calledArguments;

        public OpenedExifToolSimpleTest()
        {
            _calledArguments = new List<string>();
            _mediallionShell = A.Fake<IMedallionShell>();
            _sut = new TestableOpenedExifTool(_mediallionShell);
        }

        [Fact]
        public void ExecuteAsyncWithoutInitializingShouldThrowTest()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await _sut.ExecuteAsync(null).ConfigureAwait(false);

            // assert
            act.Should().Throw<Exception>().WithMessage("Not initialized");
        }

//        [Fact]
//        public async Task ExecuteAsyncWithoutInitializingShouldThrowTestaaaa()
//        {
//            // arrange
//
//            const int TEST_TIMEOUT = 1000;
//            var mre = new ManualResetEventSlim(false);
//            var mre2 = new ManualResetEventSlim(false);
//            A.CallTo(() => _mediallionShell.WriteLineAsync(A<string>._))
//             .Invokes(call => _calledArguments.Add((string)call.Arguments[0]))
//             .ReturnsLazily(async call =>
//                            {
//                                var line = (string)call.Arguments[0];
//                                if (line.StartsWith("-execute"))
//                                {
//                                    await Task.Yield();
//                                    mre2.Set();
//                                    if (!mre.Wait(TEST_TIMEOUT))
//                                        throw new TimeoutException();
//                                }
//                            });
//
//            _sut.Init();
//
//            // act
//            var resultTask = _sut.ExecuteAsync("arg 1").ConfigureAwait(false);
//
//            mre2.Wait(TEST_TIMEOUT);
//            mre.Set();
//
//
//            // assert
//            act.Should().Throw<Exception>().WithMessage("Not initialized");
//        }

        private class TestableOpenedExifTool : OpenedExifTool
        {
            private readonly IMedallionShell _mediallionShell;

            public TestableOpenedExifTool(IMedallionShell mediallionShell)
                : base("doesnt matter")
            {
                _mediallionShell = mediallionShell;
            }

            protected override IMedallionShell CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                return _mediallionShell;
            }
        }
    }
}