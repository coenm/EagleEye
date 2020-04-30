namespace EagleEye.ImageHash.Test.PhotoProvider
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Core;
    using EagleEye.ImageHash.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class ImageSharpPhotoSha256HashProviderTest
    {
        private const string ExistingImageFilename = "1.jpg";
        private const string ExistingImageFilenameWithoutMetadata = "1_without_metadata.jpg";
        private readonly ImageSharpPhotoSha256HashProvider sut;
        private readonly IFileService fileService;

        public ImageSharpPhotoSha256HashProviderTest()
        {
            fileService = A.Fake<IFileService>();
            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
                .ReturnsLazily(call => TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename));
            A.CallTo(() => fileService.OpenRead(ExistingImageFilenameWithoutMetadata))
                .ReturnsLazily(call => TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilenameWithoutMetadata));

            sut = new ImageSharpPhotoSha256HashProvider(fileService);

            TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename).Should().NotBeNull();
            TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilenameWithoutMetadata).Should().NotBeNull();
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

        [Fact]
        public async Task ProvideAsync_ShouldReturnSameHash_WhenFileOnlyDiffersInMetadata()
        {
            // arrange

            // act
            var result1 = await sut.ProvideAsync(ExistingImageFilename);
            var result2 = await sut.ProvideAsync(ExistingImageFilenameWithoutMetadata);

            // assert
            result1.ToArray().Should().BeEquivalentTo(result2.ToArray());
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnNull_WhenReadingImageThrows()
        {
            // arrange
            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
                .Throws(new ApplicationException("Thrown by test mock"));

            // act
            var result = await sut.ProvideAsync(ExistingImageFilename);

            // assert
            result.Should().BeEquivalentTo(new ReadOnlyMemory<byte>(null));
        }
    }
}
