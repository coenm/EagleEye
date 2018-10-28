namespace EagleEye.Picasa.Test
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Picasa.Picasa;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    public class PicasaPersonProviderTest
    {
        private const string DummyFilename = "dummy";
        private readonly PicasaPersonProvider sut;
        private readonly IPicasaService picasaService;

        public PicasaPersonProviderTest()
        {
            picasaService = A.Fake<IPicasaService>();
            sut = new PicasaPersonProvider(picasaService);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanProvideInformationUsesPicasaProviderTest(bool picasaServiceCanProvide)
        {
            // arrange
            A.CallTo(() => picasaService.CanProvideData(DummyFilename)).Returns(picasaServiceCanProvide);

            // act
            var result = sut.CanProvideInformation(DummyFilename);

            // assert
            result.Should().Be(picasaServiceCanProvide);
        }

        [Fact]
        public async Task ProvideAsyncShouldAddPersonsToMediaObjectTest()
        {
            // arrange
            var mediaObject = new MediaObject(DummyFilename);
            A.CallTo(() => picasaService.GetDataAsync(DummyFilename))
             .Returns(Task.FromResult(new FileWithPersons(DummyFilename, "Alice", "Bob")));

            // act
            await sut.ProvideAsync(DummyFilename, mediaObject).ConfigureAwait(false);

            // assert
            mediaObject.Persons.Should().BeEquivalentTo("Alice", "Bob");
        }

        [Fact]
        public async Task ProvideAsyncShouldNotThrowWhenPicasaServiceReturnsNullTest()
        {
            // arrange
            var mediaObject = new MediaObject(DummyFilename);
            A.CallTo(() => picasaService.GetDataAsync(DummyFilename))
             .Returns(Task.FromResult(null as FileWithPersons));

            // act
            await sut.ProvideAsync(DummyFilename, mediaObject).ConfigureAwait(false);

            // assert
            // Todo improve assert
            mediaObject.Should().NotBeNull();
        }
    }
}