namespace EagleEye.ExifTool.Test.PhotoProvider
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    using Sut = EagleEye.ExifTool.Parsing.DateTimeParsing;

    public class DateTimeParsingTest
    {
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

        [Theory]
        [InlineData("2012:01:23 22:13:25")]
        [InlineData("2012-01-23 22:13:25")]
        [InlineData("2012:01:23 22:13:25+00:00")]
        [InlineData("2012-01-23 22:13:25+00:00")]
        [InlineData("2012:01:23 22:13:25+01:00")]
        [InlineData("2012-01-23 22:13:25+01:00")]
        [InlineData("2012:01:23 22:13:25+05")]
        [InlineData("2012-01-23 22:13:25+05")]
        [InlineData("2012:01:23 22:13:25+5")]
        [InlineData("2012-01-23 22:13:25+5")]
        public void ParseFullDate_ShouldParseMultipleFormatsTest(string timestamp)
        {
            // arrange

            // act
            var result = Sut.ParseFullDate(timestamp);

            // assert
            result.Should().Be(new DateTime(2012, 01, 23, 22, 13, 25));
        }

        [Theory]
        [InlineData("2012 01 23 22:13:25")] // space between date instead of ':', or '-'.
        [InlineData("2012:01:23 22 13 25")] // space between time instead of ':'
        [InlineData("sdflkjsd34l*@#$ rubbish")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseFullDate_ShouldReturnNullOnWrongFormat(string timestamp)
        {
            // arrange

            // act
            var result = Sut.ParseFullDate(timestamp);

            // assert
            result.Should().BeNull();
        }
    }
}
