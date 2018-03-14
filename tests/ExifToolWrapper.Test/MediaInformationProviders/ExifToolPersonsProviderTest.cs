namespace EagleEye.ExifToolWrapper.Test.MediaInformationProviders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.ExifToolWrapper.MediaInformationProviders;

    using FakeItEasy;

    using FluentAssertions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Xunit;

    public class ExifToolPersonsProviderTest
    {
        private const string FILENAME = "DUMMY";

        private const string METADATA_XMP = @"
  ""XMP"": {
    ""PersonInImage"": [
	  ""Bob"",
	  ""Alice"",
	  ""Stephen Hawking"",
	  ""Nelson Mandela""
	]
  },";

        private const string METADATA_XMP_IPTCEXT = @"
  ""XMP-iptcExt"": {
    ""PersonInImage"": [
	  ""Bob"",
	  ""Alice"",
	  ""Stephen Hawking"",
	  ""Nelson Mandela""
	]
  },";



        private readonly ExifToolPersonsProvider _sut;
        private readonly IExifTool _exiftool;
        private readonly MediaObject _media;

        public ExifToolPersonsProviderTest()
        {
            _exiftool = A.Fake<IExifTool>();
            _sut = new ExifToolPersonsProvider(_exiftool);
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
            _media.Persons.Should().BeEmpty();
        }

        [Theory]
        [InlineData(@"""XMP-iptcExt"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Persons.Should().BeEmpty();
        }

        [Theory]
        [InlineData(METADATA_XMP)]
        [InlineData(METADATA_XMP_IPTCEXT)]
        public async Task ProvideShouldFillPersonsTest(string data)
        {
            // arrange
            var expectedPersons = new List<string>
                                      {
                                          "Bob",
                                          "Alice",
                                          "Stephen Hawking",
                                          "Nelson Mandela"
                                      };
            A.CallTo(() => _exiftool.GetMetadataAsync(FILENAME))
             .Returns(Task.FromResult(ConvertToJobject(ConvertToJsonArray(data))));

            // act
            await _sut.ProvideAsync(FILENAME, _media).ConfigureAwait(false);

            // assert
            _media.Persons.Should().BeEquivalentTo(expectedPersons);
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