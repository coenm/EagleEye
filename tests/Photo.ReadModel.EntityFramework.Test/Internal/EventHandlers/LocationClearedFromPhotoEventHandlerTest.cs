namespace Photo.ReadModel.EntityFramework.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class LocationClearedFromPhotoEventHandlerTest
    {
        private readonly LocationClearedFromPhotoEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public LocationClearedFromPhotoEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new LocationClearedFromPhotoEventHandler(eagleEyeRepository);

            savedPhotos = new List<Photo>();
            updatedPhotos = new List<Photo>();
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._))
                .Invokes(call => savedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => updatedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new LocationClearedFromPhoto(guid));

            // assert
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenPhotoDoesNotExist()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(null as Photo));

            // act
            await sut.Handle(new LocationClearedFromPhoto(guid));

            // assert
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._)).MustNotHaveHappened();
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithEmptyLocation_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new Photo
                {
                    Location = new Location
                    {
                        CountryCode = "a",
                        CountryName = "b",
                        City = "c",
                        State = "d",
                        SubLocation = "e",
                        Latitude = 11,
                        Longitude = 12,
                    },
                };

            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photoSearchResult));

            // act
            await sut.Handle(new LocationClearedFromPhoto(guid));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.Location.Should().BeNull();
        }
    }
}
