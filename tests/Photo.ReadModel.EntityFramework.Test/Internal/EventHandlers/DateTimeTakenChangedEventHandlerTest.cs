namespace Photo.ReadModel.EntityFramework.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EventHandlers;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class DateTimeTakenChangedEventHandlerTest
    {
        private readonly DateTimeTakenChangedEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly EagleEye.Photo.Domain.Aggregates.Timestamp eventDateTime;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public DateTimeTakenChangedEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new DateTimeTakenChangedEventHandler(eagleEyeRepository);

            savedPhotos = new List<Photo>();
            updatedPhotos = new List<Photo>();
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._))
                .Invokes(call => savedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => updatedPhotos.Add((Photo)call.Arguments[0]))
                .Returns(Task.FromResult(0));

            eventDateTime = new EagleEye.Photo.Domain.Aggregates.Timestamp(2021, 7, 25, 23, 55, 32);
        }

        [Fact]
        public async Task Handle_ShouldSearchForPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

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
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

            // assert
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._)).MustNotHaveHappened();
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedDateTime_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photoSearchResult = new Photo
                {
                    DateTimeTaken = null,
                };

            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photoSearchResult));

            // act
            await sut.Handle(new DateTimeTakenChanged(guid, eventDateTime));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.DateTimeTaken.Should().Be(eventDateTime.Value);
        }
    }
}
