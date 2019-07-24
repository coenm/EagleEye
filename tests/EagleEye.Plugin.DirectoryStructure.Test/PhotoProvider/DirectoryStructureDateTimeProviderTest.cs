namespace EagleEye.DirectoryStructure.Test.PhotoProvider
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.DirectoryStructure.PhotoProvider;
    using FluentAssertions;
    using Xunit;
    using Xunit.Categories;

    public class DirectoryStructureDateTimeProviderTest
    {
        private readonly DirectoryStructureDateTimeProvider sut;

        public DirectoryStructureDateTimeProviderTest()
        {
            sut = new DirectoryStructureDateTimeProvider();
        }

        [Fact]
        public void Name()
        {
            sut.Name.Should().Be("DirectoryStructureDateTimeProvider");
        }

        [Fact]
        public void Priority()
        {
            sut.Priority.Should().Be(10);
        }

        [Theory]
        [ClassData(typeof(CorrectFilenameTimestampExpectation))]
        public void CanProvideInformation_ShouldReturnTrue(string filename, Timestamp expectedTimestamp)
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(filename);

            // assert
            result.Should().BeTrue();
            expectedTimestamp.Should().NotBeNull("stupid assertion just to make this test work with two params.");
        }

        [Theory]
        [ClassData(typeof(CorrectFilenameTimestampExpectation))]
        public async Task ProvideAsync_ShouldReturnExpectedTimestamp(string filename, Timestamp expectedTimestamp)
        {
            // arrange

            // act
            var result = await sut.ProvideAsync(filename, null);

            // assert
            result.Should().Be(expectedTimestamp);
        }

        [Theory]
        [ClassData(typeof(WrongFilenameTimestampExpectation))]
        public async Task ProvideAsync_ShouldReturnNull_WhenInputDoesNotMatch(string filename)
        {
            // arrange

            // act
            var result = await sut.ProvideAsync(filename, null);

            // assert
            result.Should().BeNull();
        }

        [Theory]
        [Exploratory]
        [InlineData("2000 -file.jpg", 2000, -1, -1)]
        [InlineData("2000 file.jpg", 2000, -1, -1)]
        [InlineData("2000-10 file.jpg", 2000, 10, -1)]
        [InlineData("2000-10-31 file.jpg", 2000, 10, 31)]
        [InlineData("2000-01-31 file.jpg", 2000, 01, 31)]
        [InlineData("2000-1-31 file.jpg", 2000, 1, 31)]
        [InlineData("2000-12.31 file.jpg", 2000, 12, -1)]
        [InlineData("2000-12-311 file.jpg", 2000, 12, -1)]
        [InlineData("2000-12-31.jpg", 2000, 12, 31)]
        [InlineData("2000 12 31.jpg", 2000, 12, 31)]
        public void ExperimentRegexDateTime(string filename, int expectedYear, int expectedMonth, int expectedDay)
        {
            var regex = new Regex(
                @"^(?<year>19[\d]{2}|20[\d]{2})((?<seperator>[\. -_])(?<month>0[1-9]{1}|1[012]|[1-9])(\k<seperator>(?<day>[12][\d]|3[01]|0[1-9]|[1-9]))?)?[^\d].*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

            var result = regex.Match(filename);

            var year = result.Groups["year"];
            var month = result.Groups["month"];
            var day = result.Groups["day"];

            year.Success.Should().BeTrue();
            year.Value.Should().Be(expectedYear.ToString());

            if (expectedMonth == -1)
            {
                month.Success.Should().BeFalse();
            }
            else
            {
                month.Success.Should().BeTrue();
                var couldParse = int.TryParse(month.Value, NumberStyles.None, new NumberFormatInfo(), out var monthValue);
                couldParse.Should().BeTrue();
                monthValue.Should().Be(expectedMonth);
            }

            if (expectedDay == -1)
            {
                day.Success.Should().BeFalse();
            }
            else
            {
                day.Success.Should().BeTrue();
                var couldParse = int.TryParse(day.Value, NumberStyles.None, new NumberFormatInfo(), out var dayValue);
                couldParse.Should().BeTrue();
                dayValue.Should().Be(expectedDay);
            }
        }

        private class CorrectFilenameTimestampExpectation : TheoryData<string, Timestamp>
        {
            public CorrectFilenameTimestampExpectation()
            {
                Add("a/bb/dd/2000-file.jpg", new Timestamp(2000));
                Add("a/bb/dd/2000 file.jpg", new Timestamp(2000));
                Add("2000 file.jpg", new Timestamp(2000));
                Add("2000  file.jpg", new Timestamp(2000));
                Add("2000-11  file.jpg", new Timestamp(2000, 11));
                Add("2000-13  file.jpg", new Timestamp(2000)); // 13the month doesn't exists. Maybe it is better to ignore the year also.
                Add("1999-2  file.jpg", new Timestamp(1999, 2));
                Add("1999-02  file.jpg", new Timestamp(1999, 2));
                Add("1980-02-99  file.jpg", new Timestamp(1980, 2)); // 99the day not part of date. Maybe better to ignore found year and month.
                Add("2020-02-29 file.jpg", new Timestamp(2020, 2, 29)); // leap year
                Add("2021-02-29 file.jpg", new Timestamp(2021, 2)); // no leap year.
            }
        }

        private class WrongFilenameTimestampExpectation : TheoryData<string>
        {
            public WrongFilenameTimestampExpectation()
            {
                Add(string.Empty);
                Add("&#$KFpjdfsldf");
                Add("a/bb/dd/200-file.jpg"); // years should be 4 digits
                Add("a/bb/dd/200 file.jpg"); // years should be 4 digits
                Add("200 file.jpg"); // years should be 4 digits
                Add("200  file.jpg"); // years should be 4 digits
            }
        }
    }
}
