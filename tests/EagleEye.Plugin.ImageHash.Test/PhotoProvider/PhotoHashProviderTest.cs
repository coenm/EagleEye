namespace EagleEye.ImageHash.Test.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ImageHash.PhotoProvider;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class PhotoHashProviderTest
    {
        private const string ExistingImageFilename = "1.jpg";
        private readonly IPhotoHashProvider sut;

        public PhotoHashProviderTest()
        {
            var fileService = A.Fake<IFileService>();
            A.CallTo(() => fileService.OpenRead(ExistingImageFilename))
                .ReturnsLazily(call => TestHelper.TestImages.ReadRelativeImageFile(ExistingImageFilename));

            sut = new PhotoHashProvider(fileService);

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
        public async Task ProvideAsync_ShouldReturnThreeHashes_WhenFilenameIsValidImage()
        {
            // arrange

            // act
            var result = await sut.ProvideAsync(ExistingImageFilename);

            // assert
            result.Should().BeEquivalentTo(
                new PhotoHash
                {
                    Hash = 18442214084176449028,
                    HashName = "AverageHash",
                },
                new PhotoHash
                {
                    Hash = 3573764330010097788,
                    HashName = "DifferenceHash",
                },
                new PhotoHash
                {
                    Hash = 15585629762494286247,
                    HashName = "PerceptualHash",
                });
        }
    }
}
