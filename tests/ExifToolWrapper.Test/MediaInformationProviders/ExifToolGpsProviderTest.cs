namespace EagleEye.ExifToolWrapper.Test.MediaInformationProviders
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.ExifToolWrapper.MediaInformationProviders;

    using FakeItEasy;

    using FluentAssertions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class ExifToolGpsProviderTest
    {
        private const string FILENAME = "DUMMY";

        private const string METADATA_GPS = @"
""GPS"": {
    ""GPSLatitudeRef"": ""North"",
    ""GPSLatitude"": 40.736072,
    ""GPSLongitudeRef"": ""West"",
    ""GPSLongitude"": 73.994293,
  },";

        private const string METADATA_XMP_EXIF = @"
""XMP-exif"": {
    ""GPSLatitude"": ""+40.736072"",
    ""GPSLongitude"": -73.994293,
  }";

        private const string METADATA_COMPOSITE = @"
""Composite"": {
    ""GPSLatitude"": ""+40.736072"",
    ""GPSLatitudeRef"": ""North"",
    ""GPSLongitude"": -73.994293,
    ""GPSLongitudeRef"": ""West"",
  }";

        private readonly ExifToolGpsProvider _sut;
        private readonly IExifTool _exiftool;
        private readonly MediaObject _media;

        public ExifToolGpsProviderTest()
        {
            _exiftool = A.Fake<IExifTool>();
            _sut = new ExifToolGpsProvider(_exiftool);
            _media = new MediaObject(FILENAME);
        }

        [Fact]
        public void CanProvideInformationShouldReturnTrueTest()
        {
            // arrange

            // act
            var result = _sut.CanProvideInformation(FILENAME);

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(METADATA_GPS)]
        [InlineData(METADATA_XMP_EXIF)]
        [InlineData(METADATA_COMPOSITE)]
        public async Task ProvideShouldFillCoordinatesTest(string data)
        {
            // arrange
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Coordinate.Should().BeEquivalentTo(new Coordinate(40.736072f, -73.994293f));
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