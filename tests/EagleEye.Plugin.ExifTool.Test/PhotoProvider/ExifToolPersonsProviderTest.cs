namespace EagleEye.ExifTool.Test.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifTool;
    using EagleEye.ExifTool.PhotoProvider;
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

        private const string MetadataXpmRegion = @"
""XMP"": {
    ""XMPToolkit"": ""Image::ExifTool 8.16"",
    ""Location"": ""Dont care"",
    ""RegionInfoMP"": {
      ""Regions"": [{
        ""PersonDisplayName"": ""Bob"",
        ""Rectangle"": ""0.4775006, 0.4016632, 0.0677501, 0.1049973""
      },{
        ""PersonDisplayName"": ""Alice"",
        ""Rectangle"": ""0.2829938, 0.2569925, 0.05125505, 0.09134048""
      },{
        ""PersonDisplayName"": ""Stephen Hawking"",
        ""Rectangle"": ""0.1889983, 0.2606699, 0.04850842, 0.08265814""
      },{
        ""PersonDisplayName"": ""Nelson Mandela"",
        ""Rectangle"": ""0.1889983, 0.2606699, 0.04850842, 0.08265814""
      }]
    },
  },";

        private readonly ExifToolPersonsProvider sut;
        private readonly IExifToolReader exiftool;
        private readonly CancellationToken ct = CancellationToken.None;

        public ExifToolPersonsProviderTest()
        {
            exiftool = A.Fake<IExifToolReader>();
            sut = new ExifToolPersonsProvider(exiftool);
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
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData(@"""XMP-iptcExt"": {}")]
        public async Task ProvideCanHandleIncompleteDataTest(string data)
        {
            // arrange
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(MetadataXmp)]
        [InlineData(MetadataXmpIptcExt)]
        [InlineData(MetadataXpmRegion)]
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
            A.CallTo(() => exiftool.GetMetadataAsync(Filename, ct))
             .Returns(Task.FromResult(ConvertToJObject(ConvertToJsonArray(data))));

            // act
            var result = await sut.ProvideAsync(Filename).ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(expectedPersons);
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
