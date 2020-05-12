namespace EagleEye.ExifTool.Test.EagleEyeXmp
{
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
    using Xunit.Abstractions;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse", Justification = "Test readability")]
    public class EagleEyeMetadataWriterTest
    {
        [JetBrains.Annotations.NotNull] private readonly ITestOutputHelper output;

        private const string CorrectJson = @"
  ""XMP"": {
    ""XMPToolkit"": ""Image::ExifTool 11.97"",
    ""EagleEyeVersion"": ""1"",
    ""EagleEyeFileHash"": ""ZXM[mr?00Cg?Mdeq6[2Ay$&ASkiV)!6@C&{Pi%+%"",
    ""EagleEyeId"": ""Hshe3B/?(nN!V{}15fB5"",
    ""EagleEyeRawImageHash"": [""s?jcC6F#pPT134Ap<l&:&:TB<Po}PHtyRI0T2g.w"", ""emzo60[f$A9aE*?Ti+UAsi<AUVD3gf-<6LGgYI/{""],
    ""EagleEyeTimestamp"": ""2022:12:06 11:36:59+02:00""
  },
  ""Composite"": {
    ""ImageSize"": ""766x1024"",
    ""Megapixels"": 0.784
  }";

        private const string Filename = "DUMMY";
        private readonly EagleEyeMetadataWriter sut;
        private readonly IExifToolWriter exiftool;
        private readonly CancellationToken ct = CancellationToken.None;
        private readonly List<WriteAsyncCall> exiftoolWriteCalls;

        public EagleEyeMetadataWriterTest([JetBrains.Annotations.NotNull] ITestOutputHelper output)
        {
            this.output = output;

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
            await sut.WriteAsync("filename", new EagleEyeMetadata(), overwriteOriginal, CancellationToken.None);

            // assert
            var expected = new[]
                {
                    "-xmp-CoenmEagleEye:EagleEyeVersion=1",
                    "-xmp-CoenmEagleEye:EagleEyeId=00000000000000000000",
                    "-xmp-CoenmEagleEye:EagleEyeTimestamp=0001:01:01 00:00:00+01:00",
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
            await sut.WriteAsync("filename", new EagleEyeMetadata(), overwriteOriginal, CancellationToken.None);

            // assert
            var expected = new[]
                {
                    "-xmp-CoenmEagleEye:EagleEyeVersion=1",
                    "-xmp-CoenmEagleEye:EagleEyeId=00000000000000000000",
                    "-xmp-CoenmEagleEye:EagleEyeTimestamp=0001:01:01 00:00:00+01:00",
                    "-xmp-CoenmEagleEye:EagleEyeFileHash=",
                };
            exiftoolWriteCalls.Should().BeEquivalentTo(new WriteAsyncCall("filename", expected));
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
