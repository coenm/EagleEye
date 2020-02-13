namespace EagleEye.Picasa.Test.PhotoProvider
{
    using System.Threading.Tasks;

    using EagleEye.Picasa.PhotoProvider;
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

        [Fact]
        public void Name()
        {
            sut.Name.Should().Be("PicasaPersonProvider");
        }

        [Fact]
        public void Priority()
        {
            sut.Priority.Should().Be(10);
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
            A.CallTo(() => picasaService.CanProvideData(DummyFilename))
                .Returns(true);
            A.CallTo(() => picasaService.GetDataAsync(DummyFilename))
             .Returns(Task.FromResult(new FileWithPersons(DummyFilename, new PicasaPerson("Alice"), new PicasaPerson("Bob"))));

            // act
            var result = await sut.ProvideAsync(DummyFilename).ConfigureAwait(false);

            // assert
            result.Should().BeEquivalentTo("Alice", "Bob");
        }

        [Fact]
        public async Task ProvideAsyncShouldNotThrowWhenPicasaServiceReturnsNullTest()
        {
            // arrange
            A.CallTo(() => picasaService.CanProvideData(DummyFilename))
                .Returns(true);
            A.CallTo(() => picasaService.GetDataAsync(DummyFilename))
             .Returns(Task.FromResult(null as FileWithPersons));

            // act
            var result = await sut.ProvideAsync(DummyFilename).ConfigureAwait(false);

            // assert
            result.Should().BeNull();
        }
    }
}
