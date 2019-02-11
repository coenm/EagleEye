namespace EagleEye.Core.Test.MediaInformationProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Goal of test")]
        [Fact]
        public void ProvideAsync_ShouldThrowException_WhenMediaIsNull()
        {
            // arrange
            MediaObject nullMedia = null;

            // act
            Func<Task> act = async () => await sut.ProvideAsync("dummy", nullMedia);

            // assert
            act.Should().Throw<ArgumentNullException>();
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
            await sut.ProvideAsync(filename, media).ConfigureAwait(false);

            // assert
            media.FileInformation.Type.Should().Be(expectedMimeType);
        }
    }
}
