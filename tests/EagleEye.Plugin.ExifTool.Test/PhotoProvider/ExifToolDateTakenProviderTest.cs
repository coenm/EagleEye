namespace EagleEye.ExifTool.Test.PhotoProvider
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ExifToolDateTakenProviderTest
    {
        private const string Filename = "DUMMY";

        private const string MetadataExif = @"
  ""EXIF"": {
    ""ModifyDate"": ""2018:01:04 23:32:50"",
    ""DateTimeOriginal"": ""2018:01:01 18:05:20"",
    ""CreateDate"": ""2018:01:01 18:05:18""
  }";

        private const string MetadataXmp = @"
  ""XMP"": {
    ""DateTimeOriginal"": ""2018:01:01 15:05:27+01:00"",
    ""GPSDateTime"": ""2018:01:01 17:05:18Z"",
    ""DateCreated"": ""2018:01:01 18:05:18.347054"",
    ""CreateDate"": ""2018:01:01 18:05:18.347054"",
    ""ModifyDate"": ""2018:01:04 23:32:50+01:00""
  },";

        private readonly ExifToolDateTakenProvider sut;
        private readonly IExifToolReader exiftool;
        private readonly MediaObject media;
        private readonly CancellationToken ct = CancellationToken.None;

        public ExifToolDateTakenProviderTest()
        {
            exiftool = A.Fake<IExifToolReader>();
            sut = new ExifToolDateTakenProvider(exiftool);
            media = new MediaObject(Filename);
        }

        [Fact]
        public void CanProvideInformationShouldReturnTrueTest()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(Filename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProvideCanHandleNullResponseFromExiftoolTest()
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
             .Returns(Task.FromResult(null as JObject));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.DateTimeTaken.Should().BeNull();
        }

        [Theory]
        [InlineData(@"""Composite"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.DateTimeTaken.Should().BeNull();
        }

        [Theory]
        [InlineData(MetadataExif, 2018, 01, 01, 18, 05, 20)]
        [InlineData(MetadataXmp, 2018, 01, 01, 15, 05, 27)]
        public async Task ProvideShouldFillCoordinatesTest(string data, int year, int month, int day, int hour, int minute, int second)
        {
            // arrange
            var expectedResult = new Timestamp(year, month, day, hour, minute, second);
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.DateTimeTaken.Should().BeEquivalentTo(expectedResult);
        }

        private static string ConvertToJsonArray(string data)
        {
            return "[{ " + data + " }]";
        }

        private static JObject ConvertToJObject(string data)
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
