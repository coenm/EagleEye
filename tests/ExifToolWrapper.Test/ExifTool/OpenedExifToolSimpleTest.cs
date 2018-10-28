﻿namespace EagleEye.ExifToolWrapper.Test.ExifTool
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
        private readonly OpenedExifTool sut;
        private readonly IMedallionShell mediallionShell;
        private readonly List<string> calledArguments;

        public OpenedExifToolSimpleTest()
        {
            calledArguments = new List<string>();
            mediallionShell = A.Fake<IMedallionShell>();
            sut = new TestableOpenedExifTool(mediallionShell);
        }

        [Fact]
        public void ExecuteAsyncWithoutInitializingShouldThrowTest()
        {
            // arrange

            // act
            Func<Task> act = async () => _ = await sut.ExecuteAsync(null).ConfigureAwait(false);

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
            private readonly IMedallionShell mediallionShell;

            public TestableOpenedExifTool(IMedallionShell mediallionShell)
                : base("doesn't matter")
            {
                this.mediallionShell = mediallionShell;
            }

            protected override IMedallionShell CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
            {
                return mediallionShell;
            }
        }
    }
}
