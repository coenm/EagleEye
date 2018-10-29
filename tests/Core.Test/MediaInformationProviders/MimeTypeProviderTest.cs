namespace EagleEye.Core.Test.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.MediaInformationProviders;

    using FluentAssertions;

    using Xunit;

    public class MimeTypeProviderTest
    {
        private readonly MimeTypeProvider sut;
        private readonly MediaObject media;

        public MimeTypeProviderTest()
        {
            sut = new MimeTypeProvider();
            media = new MediaObject("a.jpg");
        }

        [Fact]
        public void CanProvideInformation_ReturnsTrueTest()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation("dummy");

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("a.jpg", "image/jpeg")]
        [InlineData("a.JPg", "image/jpeg")]
        [InlineData("a.jpeg", "image/jpeg")]
        [InlineData("a.mp4", "video/mp4")]
        [InlineData("a.mov", "video/quicktime")]
        public async Task ProvideAsync_SetsCorrectMimeTypeBasedOnFileExtensionTest(string filename, string expectedMimeType)
        {
            // arrange

            // act
            await sut.ProvideAsync(filename, media).ConfigureAwait(false);

            // assert
            media.FileInformation.Type.Should().Be(expectedMimeType);
        }
    }
}