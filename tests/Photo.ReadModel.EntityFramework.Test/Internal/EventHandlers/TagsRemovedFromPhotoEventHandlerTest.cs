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
    using Photo.ReadModel.EntityFramework.Test.Internal.EventHandlers.Helpers;
    using Xunit;

    public class TagsRemovedFromPhotoEventHandlerTest
    {
        private readonly TagsRemovedFromPhotoEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public TagsRemovedFromPhotoEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new TagsRemovedFromPhotoEventHandler(eagleEyeRepository);

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
            await sut.Handle(new TagsRemovedFromPhoto(guid));

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
            await sut.Handle(new TagsRemovedFromPhoto(guid));

            // assert
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._)).MustNotHaveHappened();
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_ShouldReIndexPhotoWithUpdatedTags_WhenPhotoExists()
        {
            // arrange
            var guid = Guid.NewGuid();
            Photo newPhoto = null;
            var photo = new Photo
            {
                Tags = TestHelpers.CreateTags("Holiday", "Zoo"),
            };

            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photo));

            // act
            await sut.Handle(new TagsRemovedFromPhoto(guid, "Zoo"));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.Tags.Should().BeEquivalentTo(TestHelpers.CreateTags("Holiday"));
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhoto_WhenPhotoAlreadyDoesNotContainTags()
        {
            // arrange
            var guid = Guid.NewGuid();
            var photo = new Photo
            {
                Tags = TestHelpers.CreateTags("Holiday", "Zoo"),
            };

            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photo));

            // act
            await sut.Handle(new TagsRemovedFromPhoto(guid, "Vacation"));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappened();
        }
    }
}
