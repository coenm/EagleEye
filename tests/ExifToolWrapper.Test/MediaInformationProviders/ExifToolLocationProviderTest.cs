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
        private const string Filename = "DUMMY";

        private const string MetadataIptcCore = @"
""XMP-iptcCore"": {
    ""CountryCode"": ""USA"",
    ""Location"": ""Union Square""
}";

        private const string MetadataPhotoshop = @"
""XMP-photoshop"": {
    ""City"": ""New-York"",
    ""Country"": ""United States"",
    ""State"": ""New York""
}";

        private const string MetadataXmp = @"
""XMP"": {
    ""CountryCode"": ""USA"",
    ""Location"": ""Union Square"",
    ""City"": ""New-York"",
    ""Country"": ""United States"",
    ""State"": ""New York""
}";

        private readonly ExifToolLocationProvider sut;
        private readonly IExifTool exiftool;
        private readonly MediaObject media;

        public ExifToolLocationProviderTest()
        {
            exiftool = A.Fake<IExifTool>();
            sut = new ExifToolLocationProvider(exiftool);
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
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(null as JObject));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Location.Should().BeEquivalentTo(new Location());
        }

        [Theory]
        [InlineData(@"""XMP"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Location.Should().BeEquivalentTo(new Location());
        }

        [Fact]
        public async Task ProvideShouldHandleXmpPhotoshopMetadataTest()
        {
            // arrange
            var data = MetadataPhotoshop;
            var expectedLocation = new Location
            {
                City = "New-York",
                State = "New York",
                CountryName = "United States",
            };
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Fact]
        public async Task ProvideShouldHandleXmpIptcCoreMetadataTest()
        {
            // arrange
            var data = MetadataIptcCore;
            var expectedLocation = new Location
                                       {
                                           SubLocation = "Union Square",
                                           CountryCode = "USA",
                                       };
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Location.Should().BeEquivalentTo(expectedLocation);
        }

        [Theory]
        [InlineData(MetadataXmp)]
        [InlineData(MetadataIptcCore + ", " + MetadataPhotoshop + ", " + MetadataXmp)]
        public async Task ProvideShouldHandleXmpMetadataTest(string data)
        {
            // arrange
            var expectedLocation = new Location
            {
                City = "New-York",
                State = "New York",
                CountryName = "United States",
                SubLocation = "Union Square",
                CountryCode = "USA",
            };
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Location.Should().BeEquivalentTo(expectedLocation);
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
