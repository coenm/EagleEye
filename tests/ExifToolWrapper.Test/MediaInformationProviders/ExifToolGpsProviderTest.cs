namespace EagleEye.ExifToolWrapper.Test.MediaInformationProviders
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.ExifToolWrapper.MediaInformationProviders;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ExifToolGpsProviderTest
    {
        private const string Filename = "DUMMY";

        private const string MetadataGps = @"
""GPS"": {
    ""GPSLatitudeRef"": ""North"",
    ""GPSLatitude"": 40.736072,
    ""GPSLongitudeRef"": ""West"",
    ""GPSLongitude"": 73.994293,
  },";

        private const string MetadataXmpExif = @"
""XMP-exif"": {
    ""GPSLatitude"": ""+40.736072"",
    ""GPSLongitude"": -73.994293,
  }";

        private const string MetadataComposite = @"
""Composite"": {
    ""GPSLatitude"": ""+40.736072"",
    ""GPSLatitudeRef"": ""North"",
    ""GPSLongitude"": -73.994293,
    ""GPSLongitudeRef"": ""West"",
  }";

        private readonly ExifToolGpsProvider sut;
        private readonly IExifTool exiftool;
        private readonly Location location;

        public ExifToolGpsProviderTest()
        {
            exiftool = A.Fake<IExifTool>();
            sut = new ExifToolGpsProvider(exiftool);
            location = new Location();
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
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(null as JObject));

            // act
            var result = await sut.ProvideAsync(Filename, location).ConfigureAwait(false);

            // assert
            result.Coordinate.Should().BeNull();
        }

        [Theory]
        [InlineData(@"""Composite"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename, location).ConfigureAwait(false);

            // assert
            result.Coordinate.Should().BeNull();
        }

        [Theory]
        [InlineData(MetadataGps)]
        [InlineData(MetadataXmpExif)]
        [InlineData(MetadataComposite)]
        public async Task ProvideShouldFillCoordinatesTest(string data)
        {
            // arrange
            var expectedGpsCoordinate = new Coordinate(40.736072f, -73.994293f);
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename, location).ConfigureAwait(false);

            // assert
            result.Coordinate.Should().BeEquivalentTo(expectedGpsCoordinate);
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
