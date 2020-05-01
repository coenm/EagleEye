namespace EagleEye.ExifTool.Test.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.EagleEyeXmp;
    using EagleEye.ExifTool.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class EagleEyeMetadataProviderTest
    {
        private const string Filename = "DUMMY";

        private const string Json = @"
   ""XMP-x"": {
    ""XMPToolkit"": ""Image::ExifTool 11.97""
  },
  ""XMP-CoenmEagleEye"": {
    ""EagleEyeFileHash"": ""Po3uzG!/]&^fFFFUZDbOp?QL0.7Cb<AK7ZBf]PdG"",
    ""EagleEyeId"": ""zFv82GPb>8M^jhj*7YC<"",
    ""EagleEyeRawImageHash"": [""Po3uzG!/]&^fFFFUZDbOp?QL0.7Cb<AK7ZBf]PdG""],
    ""EagleEyeTimestamp"": ""2022:12:06 11:36:59.123+02:00""
  },
  ""Composite"": {
    ""ImageSize"": ""766x1024"",
    ""Megapixels"": 0.784
  }";

        private readonly EagleEyeMetadataProvider sut;
        private readonly IExifTool exiftool;
        private readonly MediaObject media;

        public EagleEyeMetadataProviderTest()
        {
            exiftool = A.Fake<IExifTool>();
            sut = new EagleEyeMetadataProvider(exiftool);
            media = new MediaObject(Filename);
        }

        [Fact]
        public void CanProvideInformation_ShouldReturnTrue()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(Filename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProvideCanHandleNullResponseFromExiftoolC()
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
                .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(Json))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(
                new EagleEyeMetadata
                {
                    Id = new Guid("B9C5696E-8C84-4EFD-97CE-D72ADA148B24"),
                    Timestamp = new DateTime(2022, 12, 06, 11, 36, 59, 123),
                    RawImageHash = new List<byte[]>
                        {
                            new byte[32],
                        },
                    FileHash = new byte[32],
                });
        }


        [Fact]
        public async Task ProvideCanHandleNullResponseFromExiftool()
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
                .Returns(Task.FromResult(null as JObject));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }



        [Theory]
        [InlineData(@"""Composite"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            media.DateTimeTaken.Should().BeNull();
        }

        // [Theory]
        // [InlineData(MetadataExif, 2018, 01, 01, 18, 05, 20)]
        // [InlineData(MetadataXmp, 2018, 01, 01, 15, 05, 27)]
        // public async Task ProvideShouldFillCoordinatesTest(string data, int year, int month, int day, int hour, int minute, int second)
        // {
        //     // arrange
        //     var expectedResult = new Timestamp(year, month, day, hour, minute, second);
        //     A.CallTo(() => exiftool.GetMetadataAsync(Filename))
        //      .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));
        //
        //     // act
        //     var result = await sut.ProvideAsync(Filename, media).ConfigureAwait(false);
        //
        //     // assert
        //     media.DateTimeTaken.Should().BeEquivalentTo(expectedResult);
        // }

        [Theory]
        [InlineData("2012:01:23 22:13:25")]
        [InlineData("2012-01-23 22:13:25")]
        [InlineData("2012:01:23 22:13:25+00:00")]
        [InlineData("2012-01-23 22:13:25+00:00")]
        [InlineData("2012:01:23 22:13:25+01:00")]
        [InlineData("2012-01-23 22:13:25+01:00")]
        [InlineData("2012:01:23 22:13:25+05")]
        [InlineData("2012-01-23 22:13:25+05")]
        [InlineData("2012:01:23 22:13:25+5")]
        [InlineData("2012-01-23 22:13:25+5")]
        public void ParseFullDate_ShouldParseMultipleFormatsTest(string timestamp)
        {
            // arrange

            // act
            var result = ExifToolDateTakenProvider.ParseFullDate(timestamp);

            // assert
            result.Should().Be(new DateTime(2012, 01, 23, 22, 13, 25));
        }

        [Theory]
        [InlineData("2012 01 23 22:13:25")] // space between date instead of ':', or '-'.
        [InlineData("2012:01:23 22 13 25")] // space between time instead of ':'
        [InlineData("sdflkjsd34l*@#$ rubbish")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseFullDate_ShouldReturnNullOnWrongFormat(string timestamp)
        {
            // arrange

            // act
            var result = ExifToolDateTakenProvider.ParseFullDate(timestamp);

            // assert
            result.Should().BeNull();
        }

        private static string ConvertToJsonArray(string data)
        {
            return "[{ " + data + " }]";
        }

        private static JObject ConvertToJobject(string data)
        {
            try
            {
                var jsonResult = JsonConvert.DeserializeObject(data);
                var jsonArray = jsonResult as JArray;
                if (jsonArray?.Count != 1)
                    return null;

                return jsonArray[0] as JObject;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
