namespace EagleEye.ExifTool.Test.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.ExifTool;
    using EagleEye.ExifTool.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ExifToolTagsProviderTest
    {
        private const string Filename = "DUMMY";

        private const string MetadataXmp = @"
  ""XMP"": {
    ""Subject"": [
      ""dog"",
      ""New York"",
      ""puppy""
    ],
  },";

        private const string MetadataXmpDc = @"
  ""XMP-dc"": {
    ""Subject"": [
      ""dog"",
      ""New York"",
      ""puppy""
    ],
  },";

        private const string MetadataIptc = @"
  ""IPTC"": {
    ""Keywords"": [
      ""dog"",
      ""New York"",
      ""puppy""
    ],
  }";

        private readonly ExifToolTagsProvider sut;
        private readonly IExifToolReader exiftool;

        public ExifToolTagsProviderTest()
        {
            exiftool = A.Fake<IExifToolReader>();
            sut = new ExifToolTagsProvider(exiftool);
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
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(@"""XMP-dc"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(MetadataXmp)]
        [InlineData(MetadataXmpDc)]
        [InlineData(MetadataIptc)]
        [InlineData(MetadataXmp + MetadataIptc)]
        public async Task ProvideShouldFillTagsTest(string data)
        {
            // arrange
            var expectedTags = new List<string>
                                   {
                                       "dog",
                                       "New York",
                                       "puppy",
                                   };
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(expectedTags);
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
