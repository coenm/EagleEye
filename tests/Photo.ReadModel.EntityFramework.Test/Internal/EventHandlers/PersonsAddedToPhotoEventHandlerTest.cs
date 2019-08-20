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
    using Photo.ReadModel.EntityFramework.Test.Internal.Helpers;
    using Xunit;

    public class PersonsAddedToPhotoEventHandlerTest
    {
        private readonly PersonsAddedToPhotoEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public PersonsAddedToPhotoEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new PersonsAddedToPhotoEventHandler(eagleEyeRepository);

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
            await sut.Handle(new PersonsAddedToPhoto(guid, "Calvin", "Darion", "Eve"));

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
            await sut.Handle(new PersonsAddedToPhoto(guid, "Calvin", "Darion", "Eve"));

            // assert
            A.CallTo(() => eagleEyeRepository.SaveAsync(A<Photo>._)).MustNotHaveHappened();
            A.CallTo(() => eagleEyeRepository.UpdateAsync(A<Photo>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task HandleEvents_ShouldSaveDataToRepositoryTest()
        {
            // arrange
            var guid = Guid.NewGuid();
            var initialTags = new[] { "soccer", "sports" };
            var initialPersons = new[] { "alice", "bob" };
            var initTimestamp = DateTimeOffset.UtcNow;

            A.CallTo(() => eagleEyeRepository.GetByIdAsync(guid))
                .Returns(Task.FromResult(TestHelpers.CreatePhoto(guid, 1, string.Empty, new byte[0], initTimestamp, initialTags, initialPersons)));

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid, "Calvin", "Darion", "Eve")
                {
                    Version = 2,
                    TimeStamp = initTimestamp.AddHours(2),
                });

            // assert
            var expectedPhoto = TestHelpers.CreatePhoto(
                guid,
                2,
                string.Empty,
                new byte[0],
                initTimestamp.AddHours(2),
                initialTags,
                new[] { "alice", "bob", "Calvin", "Darion", "Eve" });

            A.CallTo(eagleEyeRepository).MustHaveHappenedTwiceExactly();
            updatedPhotos.Should().HaveCount(1);
            updatedPhotos.Single().Should().BeEquivalentTo(expectedPhoto);
        }
    }
}
