namespace EagleEye.Core.Test.DefaultImplementations.PhotoInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using FluentAssertions;
    using Xunit;

    public class MimeTypeProviderTest
    {
        private readonly MimeTypeProvider sut;

        public MimeTypeProviderTest()
        {
            sut = new MimeTypeProvider();
        }

        [Fact]
        public void Priority_ShouldBeSet()
        {
            // arrange

            // act
            var result = sut.Priority;

            // assert
            result.Should().BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void Name_ShouldNotBeNullOrWhiteSpace()
        {
            // arrange

            // act
            var result = sut.Name;

            // assert
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void CanProvideInformation_ShouldReturnTrue_WhenFilenameIsNotNullOrEmpty()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation("dummy");

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void CanProvideInformation_ShouldReturnFalse_WhenFilenameIsNullOrEmpty(string filename)
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(filename);

            // assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("a.jpg", "image/jpeg")]
        [InlineData("a.JPg", "image/jpeg")]
        [InlineData("a.jpeg", "image/jpeg")]
        [InlineData("a.mp4", "video/mp4")]
        [InlineData("a.mov", "video/quicktime")]
        public async Task ProvideAsync_ShouldSetsCorrectMimeTypeBasedOnFileExtensionTest(string filename, string expectedMimeType)
        {
            // arrange

            // act
            var result = await sut.ProvideAsync(filename).ConfigureAwait(false);

            // assert
            result.Should().Be(expectedMimeType);
        }
    }
}
