namespace EagleEye.ImageHash.Test.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.ImageHash.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ImageSharpPhotoSha256HashProviderTest
    {
        private const string ExistingImageFilename = "1.jpg";
        private readonly ImageSharpPhotoSha256HashProvider sut;

        public ImageSharpPhotoSha256HashProviderTest()
        {
            var fileService = A.Fake<IFileService>();
            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
                .ReturnsLazily(call => TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename));

            sut = new ImageSharpPhotoSha256HashProvider(fileService);

            TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename).Should().NotBeNull();
        }

        [Fact]
        public void Priority_DoesNotThrow()
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
        public void CanProvideInformation_ShouldReturnTrue()
        {
            // arrange

            // act
            var result = sut.CanProvideInformation(ExistingImageFilename);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnImageHash_WhenImageExists()
        {
            // arrange

            // act
            var result = await sut.ProvideAsync(ExistingImageFilename);

            // assert
            result.Should().NotBeNull();
            CoenM.Encoding.Z85.Encode(result.ToArray()).Should().Be("+]JP}/TH1-hP/ax&a)iqy%H<Ze>cpbBhph)ggipp");
        }
    }
}
