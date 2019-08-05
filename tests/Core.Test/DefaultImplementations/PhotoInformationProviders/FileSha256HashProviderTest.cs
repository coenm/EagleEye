namespace EagleEye.Core.Test.DefaultImplementations.PhotoInformationProviders
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using EagleEye.Core.DefaultImplementations.PhotoInformationProviders;
    using EagleEye.Core.Interfaces.Core;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class FileSha256HashProviderTest
    {
        private readonly FileSha256HashProvider sut;
        private readonly IFileService fileService;

        public FileSha256HashProviderTest()
        {
            fileService = A.Fake<IFileService>();
            sut = new FileSha256HashProvider(fileService);
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

        [Fact]
        public async Task ProvideAsync_ShouldOpenReadFile()
        {
            // arrange

            // act
            var result = await sut.ProvideAsync("dummy.jpg").ConfigureAwait(false);

            // assert
            A.CallTo(() => fileService.OpenRead("dummy.jpg")).MustHaveHappenedOnceExactly();
            A.CallTo(() => fileService.OpenRead(A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnHashOfFile_WhenFileCanBeRead()
        {
            // arrange
            A.CallTo(() => fileService.OpenRead("dummy.jpg"))
                .ReturnsLazily(call => new MemoryStream(new byte[] { 0x34, 0xA2 }));

            // act
            var result = await sut.ProvideAsync("dummy.jpg").ConfigureAwait(false);

            // assert
            result.Should().NotBeNull();
            CoenM.Encoding.Z85.Encode(result.ToArray()).Should().Be("wpMknFe:u(3gNydx@^/KiX&+=W7EA-5+BC1/N@7X");
        }

        [Fact]
        public async Task ProvideAsync_ShouldReturnHashOfFile_WhenFileCanNotBeRead()
        {
            // arrange
            A.CallTo(() => fileService.OpenRead("dummy.jpg"))
                .Throws<FileNotFoundException>();

            // act
            var result = await sut.ProvideAsync("dummy.jpg").ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo(new ReadOnlyMemory<byte>(null));
        }
    }
}
