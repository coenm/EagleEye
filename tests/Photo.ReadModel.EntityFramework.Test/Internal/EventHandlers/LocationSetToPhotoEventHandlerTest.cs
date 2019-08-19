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

    using Location = EagleEye.Photo.Domain.Aggregates.Location;

    public class LocationSetToPhotoEventHandlerTest
    {
        private readonly Location eventLocation;
        private readonly LocationSetToPhotoEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public LocationSetToPhotoEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new LocationSetToPhotoEventHandler(eagleEyeRepository);

            savedPhotos = new List<Photo>();
            updatedPhotos = new List<Photo>();
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._))
                .Invokes(call => savedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => updatedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));

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
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_ShouldDoNothing_WhenPhotoDoesNotExist()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(null as Photo));

            // act
            await sut.Handle(new LocationSetToPhoto(guid, eventLocation));

            // assert
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._)).MustNotHaveHappened();
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedLocation_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new Photo
            {
                Location = new EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models.Location
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
            await sut.Handle(new LocationSetToPhoto(guid, eventLocation));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            var location = newPhoto.Location;
            location.Should().NotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            location.CountryCode.Should().Be(eventLocation.CountryCode);
            location.CountryName.Should().Be(eventLocation.CountryName);
            location.City.Should().Be(eventLocation.City);
            location.State.Should().Be(eventLocation.State);
            location.SubLocation.Should().Be(eventLocation.SubLocation);
            location.Latitude.Should().Be(eventLocation.Latitude);
            location.Longitude.Should().Be(eventLocation.Longitude);
        }
    }
}
