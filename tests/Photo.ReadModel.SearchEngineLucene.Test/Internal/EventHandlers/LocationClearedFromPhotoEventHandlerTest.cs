namespace Photo.ReadModel.SearchEngineLucene.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class LocationClearedFromPhotoEventHandlerTest
    {
        private readonly LocationClearedFromPhotoEventHandler sut;
        private readonly IPhotoIndex photoIndex;

        public LocationClearedFromPhotoEventHandlerTest()
        {
            photoIndex = A.Fake<IPhotoIndex>();
            sut = new LocationClearedFromPhotoEventHandler(photoIndex);
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new LocationClearedFromPhoto(guid));

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
            await sut.Handle(new LocationClearedFromPhoto(guid));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithEmptyLocation_WhenPhotoExists()
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
            await sut.Handle(new LocationClearedFromPhoto(guid));

            // assert
            A.CallTo(() => photoIndex.ReIndexMediaFileAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.LocationCountryCode.Should().BeNull();
            newPhoto.LocationCountryName.Should().BeNull();
            newPhoto.LocationCity.Should().BeNull();
            newPhoto.LocationState.Should().BeNull();
            newPhoto.LocationSubLocation.Should().BeNull();
            newPhoto.LocationLatitude.Should().BeNull();
            newPhoto.LocationLongitude.Should().BeNull();
        }
    }
}
