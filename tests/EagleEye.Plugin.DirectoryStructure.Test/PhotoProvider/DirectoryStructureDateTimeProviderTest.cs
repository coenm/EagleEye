namespace EagleEye.DirectoryStructure.Test.PhotoProvider
{
    using EagleEye.DirectoryStructure.PhotoProvider;
    using FluentAssertions;
    using Xunit;

    public class DirectoryStructureDateTimeProviderTest
    {
        private const string DummyFilename = "dummy";
        private readonly DirectoryStructureDateTimeProvider sut;

        public DirectoryStructureDateTimeProviderTest()
        {
            sut = new DirectoryStructureDateTimeProvider();
        }

        [Fact]
        public void CanProvideInformation_ShouldReturnTrue()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(DummyFilename);

            // assert
            result.Should().BeFalse();
        }
    }
}
