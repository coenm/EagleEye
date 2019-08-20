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
    using Photo.ReadModel.EntityFramework.Test.Internal.Helpers;
    using Xunit;

    public class TagsAddedToPhotoEventHandlerTest
    {
        private readonly TagsAddedToPhotoEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public TagsAddedToPhotoEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new TagsAddedToPhotoEventHandler(eagleEyeRepository);

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
            await sut.Handle(new TagsAddedToPhoto(guid, "Zoo", "Vacation", "Holiday"));

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
            await sut.Handle(new TagsAddedToPhoto(guid, "Zoo", "Vacation", "Holiday"));

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
                Tags = TestHelpers.CreateTags("Vacation"),
            };

            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._))
                .Invokes(call => { newPhoto = call.Arguments[0] as Photo; });
            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photo));

            // act
            await sut.Handle(new TagsAddedToPhoto(guid, "Zoo", "Vacation", "Holiday"));

            // assert
            var expectedTags = TestHelpers.CreateTags("Holiday", "Zoo", "Vacation");
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappenedOnceExactly();
            newPhoto.Should().NotBeNull();
            newPhoto.Tags.Should().BeEquivalentTo(expectedTags);
        }

        [Fact]
        public async Task Handle_ShouldUpdatePhoto_WhenPhotoAlreadyContainedUpdatedTags()
        {
            // arrange
            var guid = Guid.NewGuid();
            var photo = new Photo
            {
                Tags = TestHelpers.CreateTags("Holiday", "Zoo"),
            };

            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid)).Returns(Task.FromResult(photo));

            // act
            await sut.Handle(new TagsAddedToPhoto(guid, "Zoo"));

            // assert
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustHaveHappened();
        }
    }
}
