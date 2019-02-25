namespace EagleEye.DirectoryStructure.Test.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.DirectoryStructure.PhotoProvider;
    using FluentAssertions;
    using Xunit;

    public class DirectoryStructureDateTimeProviderTest
    {
        private readonly DirectoryStructureDateTimeProvider sut;

        public DirectoryStructureDateTimeProviderTest()
        {
            sut = new DirectoryStructureDateTimeProvider();
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
        [ClassData(typeof(WrongFilenameTimestampExpectation))]
        public void CanProvideInformation_ShouldReturnFalse(string filename)
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(filename);

            // assert
            result.Should().BeFalse();
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

        private class CorrectFilenameTimestampExpectation : TheoryData<string, Timestamp>
        {
            public CorrectFilenameTimestampExpectation()
            {
                Add("a/bb/dd/2000-file.jpg", new Timestamp(2000));
                Add("a/bb/dd/2000 file.jpg", new Timestamp(2000));
                Add("2000 file.jpg", new Timestamp(2000));
                Add("2000  file.jpg", new Timestamp(2000));
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
