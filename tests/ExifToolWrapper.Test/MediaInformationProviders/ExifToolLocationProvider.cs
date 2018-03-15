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

    public class ExifToolLocationProviderTest
    {
        private const string FILENAME = "DUMMY";

        private const string METADATA_IPTC_CORE = @"
""XMP-iptcCore"": {
    ""CountryCode"": ""USA"",
    ""Location"": ""Union Square""
}";

        private const string METADATA_PHOTOSHOP = @"
""XMP-photoshop"": {
    ""City"": ""New-York"",
    ""Country"": ""United States"",
    ""State"": ""New York""
}";

        private const string METADATA_XMP = @"
""XMP"": {
    ""CountryCode"": ""USA"",
    ""Location"": ""Union Square"",
    ""City"": ""New-York"",
    ""Country"": ""United States"",
    ""State"": ""New York""
}";

        private readonly ExifToolLocationProvider _sut;
        private readonly IExifTool _exiftool;
        private readonly MediaObject _media;

        public ExifToolLocationProviderTest()
        {
            _exiftool = A.Fake<IExifTool>();
            _sut = new ExifToolLocationProvider(_exiftool);
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

        [Fact]
        public async Task ProvideCanHandleNullResponseFromExiftoolTest()
        {
            // arrange
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(null as JObject));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Should().BeEquivalentTo(new Location());
        }

        [Theory]
        [InlineData(@"""XMP"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Should().BeEquivalentTo(new Location());
        }

        [Fact]
        public async Task ProvideShouldHandleXmpPhotoshopMetadataTest()
        {
            // arrange
            var data = METADATA_PHOTOSHOP;
            var expectedLocation = new Location
                                       {
                                           City = "New-York",
                                           State = "New York",
                                           CountryName = "United States"
                                       };
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Fact]
        public async Task ProvideShouldHandleXmpIptcCoreMetadataTest()
        {
            // arrange
            var data = METADATA_IPTC_CORE;
            var expectedLocation = new Location
                                       {
                                           SubLocation = "Union Square",
                                           CountryCode = "USA",
                                       };
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Theory]
        [InlineData(METADATA_XMP)]
        [InlineData(METADATA_IPTC_CORE + ", " + METADATA_PHOTOSHOP + ", " + METADATA_XMP)]
        public async Task ProvideShouldHanldeXmpMetadataTest(string data)
        {
            // arrange
            var expectedLocation = new Location
                                       {
                                           City = "New-York",
                                           State = "New York",
                                           CountryName = "United States",
                                           SubLocation = "Union Square",
                                           CountryCode = "USA"
                                       };
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Location.Should().BeEquivalentTo(expectedLocation);
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