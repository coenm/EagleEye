namespace EagleEye.ExifTool.Test.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.EagleEyeXmp;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "Test readability")]
    public class EagleEyeMetadataWriterTest
    {
        private readonly EagleEyeMetadataWriter sut;
        private readonly IExifToolWriter exiftool;
        private readonly List<WriteAsyncCall> exiftoolWriteCalls;

        public EagleEyeMetadataWriterTest()
        {
            exiftool = A.Fake<IExifToolWriter>();
            sut = new EagleEyeMetadataWriter(exiftool);

            exiftoolWriteCalls = new List<WriteAsyncCall>();
            A.CallTo(() => exiftool.WriteAsync(A<string>._, A<IEnumerable<string>>._, A<CancellationToken>._))
                .Invokes(call =>
                {
                    var filename = call.Arguments[0] as string;
                    var arguments = call.Arguments[1] as IEnumerable<string>;

                    exiftoolWriteCalls.Add(new WriteAsyncCall(filename, arguments));
                });
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteToExiftool_WhenEmptyDataProvidedAndOverwriteOriginalTrue()
        {
            // arrange
            var overwriteOriginal = true;

            // act
            await sut.WriteAsync("filename", CreateEmptyEagleEyeMetadata(), overwriteOriginal, CancellationToken.None);

            // assert
            var expected = new[]
                {
                    "-xmp-CoenmEagleEye:EagleEyeVersion=1",
                    "-xmp-CoenmEagleEye:EagleEyeId=00000000000000000000",
                    "-xmp-CoenmEagleEye:EagleEyeTimestamp=0001:01:01 00:00:00+00:00",
                    "-xmp-CoenmEagleEye:EagleEyeFileHash=",
                    "-overwrite_original",
                };
            exiftoolWriteCalls.Should().BeEquivalentTo(new WriteAsyncCall("filename", expected));
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteToExiftool_WhenEmptyDataProvidedAndOverwriteOriginalFalse()
        {
            // arrange
            var overwriteOriginal = false;

            // act
            await sut.WriteAsync("filename", CreateEmptyEagleEyeMetadata(), overwriteOriginal, CancellationToken.None);

            // assert
            var expected = new[]
                {
                    "-xmp-CoenmEagleEye:EagleEyeVersion=1",
                    "-xmp-CoenmEagleEye:EagleEyeId=00000000000000000000",
                    "-xmp-CoenmEagleEye:EagleEyeTimestamp=0001:01:01 00:00:00+00:00",
                    "-xmp-CoenmEagleEye:EagleEyeFileHash=",
                };
            exiftoolWriteCalls.Should().BeEquivalentTo(new WriteAsyncCall("filename", expected));
        }

        private static EagleEyeMetadata CreateEmptyEagleEyeMetadata()
        {
            return new EagleEyeMetadata
            {
                // with set timestamp to make sure test runs on CI in other timezone
                Timestamp = DateTime.MinValue.ToUniversalTime(),
            };
        }

        private class WriteAsyncCall
        {
            public WriteAsyncCall(string filename, IEnumerable<string> arguments)
            {
                Filename = filename;
                Arguments = arguments?.ToList() ?? new List<string>(0);
            }

            public string Filename { get; }

            public List<string> Arguments { get; }
        }
    }
}
