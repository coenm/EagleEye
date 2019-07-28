namespace EagleEye.DirectoryStructure.Test.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.DirectoryStructure.PhotoProvider;
    using FluentAssertions;
    using Xunit;

    public class MobileFilenameDateTimeProviderTest
    {
        private readonly MobileFilenameDateTimeProvider sut;

        public MobileFilenameDateTimeProviderTest()
        {
            sut = new MobileFilenameDateTimeProvider();
        }

        [Fact]
        public void Name()
        {
            sut.Name.Should().Be("MobileFilenameDateTimeProvider");
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

        [Fact]
        public async Task ProvideAsync_ShouldReturnNull_WhenFilenameCannotBeRead()
        {
            // arrange
            // Invalid filename characters (generated using  string.Join("", System.IO.Path.GetInvalidFileNameChars())  )
            const string invalidFilenameChars = "\"<>|\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\t\n\v\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f:*?\\/";
            const string filename = "a/bb/dd/IMG-20170325-WA0014-" + invalidFilenameChars + "file.jpg";

            // act
            var result = await sut.ProvideAsync(filename, null);

            // assert
            result.Should().BeNull("filename contained invalid characters");
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

        private class CorrectFilenameTimestampExpectation : TheoryData<string, Timestamp>
        {
            public CorrectFilenameTimestampExpectation()
            {
                Add("a/bb/dd/IMG-20170325-WA0014.jpg", new Timestamp(2017, 3, 25));
                Add("a/bb/dd/VID-20161220-WA0001.mp4", new Timestamp(2016, 12, 20));
                Add("a/bb/dd/20150905_183425.jpg", new Timestamp(2015, 09, 05)); // should have time (todo)
            }
        }

        private class WrongFilenameTimestampExpectation : TheoryData<string>
        {
            public WrongFilenameTimestampExpectation()
            {
                Add(string.Empty);
                Add("&#$KFpjdfsldf");
                // Add("a/bb/dd/IMG-201703251-WA0014.jpg"); // one digit to many (todo to fix)
                Add("a/bb/dd/IMG-2017032-WA0015.jpg"); // one digit to less
            }
        }
    }
}
