namespace EagleEye.ExifToolWrapper.Test.MediaInformationProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Data;
    using EagleEye.ExifToolWrapper.MediaInformationProviders;

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
        private readonly IExifTool exiftool;
        private readonly MediaObject media;

        public ExifToolTagsProviderTest()
        {
            exiftool = A.Fake<IExifTool>();
            sut = new ExifToolTagsProvider(exiftool);
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
            media.Tags.Should().BeEmpty();
        }

        [Theory]
        [InlineData(@"""XMP-dc"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Tags.Should().BeEmpty();
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
            await sut.ProvideAsync(Filename, media).ConfigureAwait(false);

            // assert
            media.Tags.Should().BeEquivalentTo(expectedTags);
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
