namespace Photo.EntityFramework.ReadModel.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Domain.Events;

    using FakeItEasy;

    using FluentAssertions;

    using Photo.EntityFramework.ReadModel.Internal.EntityFramework;
    using Photo.EntityFramework.ReadModel.Internal.EntityFramework.Models;
    using Photo.EntityFramework.ReadModel.Internal.EventHandlers;

    using Xunit;

    public class MediaItemConsistencyTest
    {
        private readonly MediaItemConsistency sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public MediaItemConsistencyTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new MediaItemConsistency(eagleEyeRepository);

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
        public async Task HandlePhotoCreated_ShouldSaveDataToRepositoryTest()
        {
            // arrange
            var guid = Guid.NewGuid();
            var version = 0;
            var filename = "a.jpg";
            var fileMimeType = "image/jpeg";
            var initialTags = new[] { "soccer", "sports" };
            var initialPersons = new[] { "alice", "bob" };
            var initTimestamp = DateTimeOffset.UtcNow;
            var fileHash = new byte[32];
            var expectedPhoto = CreatePhoto(guid, version, filename, fileHash, initTimestamp, initialTags, initialPersons);

            // act
            await sut.Handle(new PhotoCreated(guid, filename, fileMimeType, fileHash, initialTags, initialPersons)
            {
                TimeStamp = initTimestamp,
            }).ConfigureAwait(false);

            // assert
            A.CallTo(eagleEyeRepository).MustHaveHappenedOnceExactly();
            savedPhotos.Should().HaveCount(1);
            savedPhotos.Single().Should().BeEquivalentTo(expectedPhoto);
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
                .Returns(Task.FromResult(CreatePhoto(guid, 1, string.Empty, new byte[0], initTimestamp, initialTags, initialPersons)));

            // act
            await sut.Handle(new PersonsAddedToPhoto(guid, "Calvin", "Darion", "Eve")
            {
                Version = 2,
                TimeStamp = initTimestamp.AddHours(2),
            });

            // assert
            var expectedPhoto = CreatePhoto(
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

        private static Photo CreatePhoto(Guid id, int version, string filename, byte[] fileSha, DateTimeOffset eventTimestamp, string[] tags, string[] people)
        {
            return new Photo
                   {
                       Id = id,
                       Version = version,
                       Filename = filename,
                       FileSha256 = fileSha,
                       EventTimestamp = eventTimestamp,
                       Tags = CreateTags(tags),
                       People = CreatePeoples(people),
                       FileMimeType = "image/jpeg",
                   };
        }

        private static List<Tag> CreateTags(params string[] tags)
        {
            return tags?.Select(x => new Tag { Value = x }).ToList();
        }

        private static List<Person> CreatePeoples(params string[] people)
        {
            return people?.Select(x => new Person { Value = x }).ToList();
        }
    }
}
