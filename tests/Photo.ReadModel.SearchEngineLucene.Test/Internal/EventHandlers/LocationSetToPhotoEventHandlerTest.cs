namespace Photo.ReadModel.SearchEngineLucene.Test.Internal.EventHandlers
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class LocationSetToPhotoEventHandlerTest
    {
        private readonly LocationSetToPhotoEventHandler sut;
        private readonly IPhotoIndex photoIndex;
        private readonly Location eventLocation;

        public LocationSetToPhotoEventHandlerTest()
        {
            photoIndex = A.Fake<IPhotoIndex>();
            sut = new LocationSetToPhotoEventHandler(photoIndex);
            eventLocation = new Location(
                "NLD",
                "Netherlands",
                "Utrecht",
                "Utrecht",
                "Centrum",
                12,
                34);
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new LocationSetToPhoto(guid, eventLocation));

            // assert
            A.CallTo(() => photoIndex.Search(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenPhotoDoesNotExist()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => photoIndex.Search(guid)).Returns(null);

            // act
            await sut.Handle(new LocationSetToPhoto(guid, eventLocation));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedLocation_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new PhotoSearchResult(1)
            {
                LocationCountryCode = "a",
                LocationCountryName = "b",
                LocationCity = "c",
                LocationState = "d",
                LocationSubLocation = "e",
                LocationLatitude = 11,
                LocationLongitude = 12,
            };

            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => photoIndex.Search(guid)).Returns(photoSearchResult);

            // act
            await sut.Handle(new LocationSetToPhoto(guid, eventLocation));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.LocationCountryCode.Should().Be(eventLocation.CountryCode);
            newPhoto.LocationCountryName.Should().Be(eventLocation.CountryName);
            newPhoto.LocationCity.Should().Be(eventLocation.City);
            newPhoto.LocationState.Should().Be(eventLocation.State);
            newPhoto.LocationSubLocation.Should().Be(eventLocation.SubLocation);
            newPhoto.LocationLatitude.Should().Be(eventLocation.Latitude);
            newPhoto.LocationLongitude.Should().Be(eventLocation.Longitude);
        }
    }
}
