namespace EagleEye.Core.Test.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core.MediaInformationProviders;

    using FluentAssertions;

    using Xunit;

    public class MimeTypeProviderTest
    {
        private readonly MimeTypeProvider _sut;
        private readonly MediaObject _media;

        public MimeTypeProviderTest()
        {
            _sut = new MimeTypeProvider();
            _media = new MediaObject("a.jpg");
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
            await _sut.ProvideAsync(filename, _media).ConfigureAwait(false);

            // assert
            _media.FileInformation.Type.Should().Be(expectedMimeType);
        }
    }
}