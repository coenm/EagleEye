namespace EagleEye.ExifToolWrapper.Test.MediaInformationProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.MediaInformationProviders;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ExifToolPersonsProviderTest
    {
        private const string Filename = "DUMMY";

        private const string MetadataXmp = @"
  ""XMP"": {
    ""PersonInImage"": [
	  ""Bob"",
	  ""Alice"",
	  ""Stephen Hawking"",
	  ""Nelson Mandela""
	]
  },";

        private const string MetadataXmpIptcExt = @"
  ""XMP-iptcExt"": {
    ""PersonInImage"": [
	  ""Bob"",
	  ""Alice"",
	  ""Stephen Hawking"",
	  ""Nelson Mandela""
	]
  },";

        private readonly ExifToolPersonsProvider sut;
        private readonly IExifTool exiftool;
        private readonly List<string> persons;

        public ExifToolPersonsProviderTest()
        {
            exiftool = A.Fake<IExifTool>();
            sut = new ExifToolPersonsProvider(exiftool);
            persons = new List<string>();
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
            await sut.ProvideAsync(Filename, persons).ConfigureAwait(false);

            // assert
            persons.Should().BeEmpty();
        }

        [Theory]
        [InlineData(@"""XMP-iptcExt"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, persons).ConfigureAwait(false);

            // assert
            persons.Should().BeEmpty();
        }

        [Theory]
        [InlineData(MetadataXmp)]
        [InlineData(MetadataXmpIptcExt)]
        public async Task ProvideShouldFillPersonsTest(string data)
        {
            // arrange
            var expectedPersons = new List<string>
            {
                "Bob",
                "Alice",
                "Stephen Hawking",
                "Nelson Mandela",
            };
            A.CallTo(() => exiftool.GetMetadataAsync(Filename))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            await sut.ProvideAsync(Filename, persons).ConfigureAwait(false);

            // assert
            persons.Should().BeEquivalentTo(expectedPersons);
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
