﻿namespace EagleEye.Picasa.Test
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Picasa.Picasa;

    using FakeItEasy;

    using FluentAssertions;

    using Xunit;

    public class PicasaPersonProviderTest
    {
        private const string DUMMY_FILENAME = "dummy";
        private readonly PicasaPersonProvider _sut;
        private readonly IPicasaService _picasaService;

        public PicasaPersonProviderTest()
        {
            _picasaService = A.Fake<IPicasaService>();
            _sut = new PicasaPersonProvider(_picasaService);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanProvideInformationUsesPicasaProviderTest(bool picasaServiceCanProvide)
        {
            // arrange
            A.CallTo(() => _picasaService.CanProvideData(DUMMY_FILENAME)).Returns(picasaServiceCanProvide);

            // act
            var result = _sut.CanProvideInformation(DUMMY_FILENAME);

            // assert
            result.Should().Be(picasaServiceCanProvide);
        }

        [Fact]
        public async Task ProvideAsyncShouldAddPersonsToMediaObjectTest()
        {
            // arrange
            var mediaObject = new MediaObject(DUMMY_FILENAME);
            A.CallTo(() => _picasaService.GetDataAsync(DUMMY_FILENAME))
             .Returns(Task.FromResult(new FileWithPersons(DUMMY_FILENAME, "Alice", "Bob")));

            // act
            await _sut.ProvideAsync(DUMMY_FILENAME, mediaObject).ConfigureAwait(false);

            // assert
            mediaObject.Persons.Should().BeEquivalentTo("Alice", "Bob");
        }

        [Fact]
        public async Task ProvideAsyncShouldNotThrowWhenPicasaServiceReturnsNullTest()
        {
            // arrange
            var mediaObject = new MediaObject(DUMMY_FILENAME);
            A.CallTo(() => _picasaService.GetDataAsync(DUMMY_FILENAME))
             .Returns(Task.FromResult(null as FileWithPersons));

            // act
            await _sut.ProvideAsync(DUMMY_FILENAME, mediaObject).ConfigureAwait(false);

            // assert
            // Todo improve assert
            mediaObject.Should().NotBeNull();
        }
    }
}