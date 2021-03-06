﻿namespace EagleEye.ExifTool.Test.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.EagleEyeXmp;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class EagleEyeMetadataProviderTest
    {
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
        private readonly EagleEyeMetadataProvider sut;
        private readonly IExifToolReader exiftool;
        private readonly CancellationToken ct = CancellationToken.None;

        public EagleEyeMetadataProviderTest()
        {
            exiftool = A.Fake<IExifToolReader>();
            sut = new EagleEyeMetadataProvider(exiftool);
        }

        [Fact]
        public void Name_ShouldBeNameOfClass()
        {
            // arrange
            var expectedName = sut.GetType().Name;

            // act
            var result = sut.Name;

            // assert
            result.Should().Be(expectedName);
            result.Should().Be("EagleEyeMetadataProvider");
        }

        [Fact]
        public void Priority_ShouldNotThrow()
        {
            // arrange

            // act
            Action act = () => _ = sut.Priority;

            // assert
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CanProvideInformation_ShouldReturnFalse_WhenFilenameIsNullOrEmpty(string filename)
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(filename);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanProvideInformation_ShouldReturnTrue_WhenFilenameIsNotEmpty()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(Filename);

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("\"EagleEyeVersion\": \"1\",")] // version is a string
        [InlineData("\"EagleEyeVersion\": 1,")] // version is an integer
        public async Task ProvideAsync_ShouldReturnMetadata_WhenEagleEyeVersionIsOneAndDataIsComplete(string replacement)
        {
            // arrange
            string json = GenerateJson("\"EagleEyeVersion\": \"1\",", replacement);

            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
                .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(json))));

            // act
            var result = await sut.ProvideAsync(Filename, ct).ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(
                new EagleEyeMetadata
                {
                    Id = new Guid("1990D286-AD75-43B0-9AF9-0011034D12F7"),
                    Timestamp = new DateTime(2022, 12, 06, 11, 36, 59),
                    RawImageHash = new List<byte[]>
                        {
                            new byte[]
                            {
                                0x59, 0xb9, 0xf3, 0x74, 0x14, 0x34, 0x9b, 0x4f, 0xab, 0x2a, 0x37, 0x17, 0x50, 0x77, 0x6f, 0xb3,
                                0xe2, 0x63, 0x29, 0xd9, 0x9f, 0x98, 0x38, 0x65, 0x5b, 0x7f, 0x89, 0xf0, 0xab, 0x35, 0x18, 0x27,
                            },
                            new byte[]
                            {
                                0x2c, 0x61, 0x4d, 0xd5, 0x02, 0xd3, 0x50, 0x53, 0x1c, 0x62, 0xdf, 0xe8, 0xab, 0xd0, 0x6b, 0xe6,
                                0x57, 0xcf, 0x48, 0x73, 0xb2, 0xc7, 0x5b, 0x8e, 0xc6, 0xb1, 0x72, 0x3f, 0x33, 0xff, 0x7b, 0xa0,
                            },
                        },
                    FileHash = new byte[]
                        {
                            0xbf, 0xf9, 0xe3, 0x23, 0x56, 0x9b, 0x52, 0x8c, 0x34, 0x66, 0xed, 0xda, 0x51, 0x26, 0x31, 0x7b,
                            0x6c, 0xdb, 0x62, 0x8b, 0x3e, 0xe9, 0x6c, 0xaf, 0x15, 0xa6, 0x6d, 0xd0, 0x9f, 0x60, 0x3c, 0x16,
                        },
                });
        }

        [Theory]
        [InlineData("\"EagleEyeVersion\": \" 1 \",")]
        [InlineData("\"EagleEyeVersion\": \"2\",")]
        [InlineData("\"EagleEyeVersion\": \"\",")]
        [InlineData("\"EagleEyeVersion\": \"aaa1\",")]
        [InlineData("\"EagleEyeVersion\": 3,")]
        public async Task ProvideAsync_ShouldReturnNull_WhenEagleEyeVersionIsNotOne(string replacement)
        {
            // arrange
            string json = GenerateJson("\"EagleEyeVersion\": \"1\",", replacement);

            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
                .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(json))));

            // act
            var result = await sut.ProvideAsync(Filename, ct).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("\"EagleEyeVersionX\": \"1\",")] // key name is not ok
        [InlineData("")]
        public async Task ProvideAsync_ShouldReturnNull_WhenNoEagleEyeVersionKeyFound(string replacement)
        {
            // arrange
            string json = GenerateJson("\"EagleEyeVersion\": \"1\",", replacement);

            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
                .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(json))));

            // act
            var result = await sut.ProvideAsync(Filename, ct).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnNull_WhenXmpMetadataFound()
        {
            // arrange
            const string json = @"
  ""Composite"": {
    ""ImageSize"": ""766x1024"",
    ""Megapixels"": 0.784
  }";

            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
                .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(json))));

            // act
            var result = await sut.ProvideAsync(Filename, ct).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnNull_WhenExiftoolReturnsNull()
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
                .Returns(Task.FromResult(null as JObject));

            // act
            var result = await sut.ProvideAsync(Filename, ct).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        private static string ConvertToJsonArray(string data)
        {
            return "[{ " + data + " }]";
        }

        private static JObject ConvertToJObject(string data)
        {
            var jsonResult = JsonConvert.DeserializeObject(data);
            var jsonArray = jsonResult as JArray;
            if (jsonArray?.Count != 1)
                return null;

            return jsonArray[0] as JObject;
        }

        private string GenerateJson(string search, string replace)
        {
            if (CorrectJson.Contains(search))
                return CorrectJson.Replace(search, replace);
            throw new Exception($"Search string '{search}' not found.");
        }
    }
}
