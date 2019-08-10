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

    public class PhotoCreatedEventHandlerTest
    {
        private readonly PhotoCreatedEventHandler sut;
        private readonly IEagleEyeRepository eagleEyeRepository;
        private readonly List<Photo> savedPhotos;
        private readonly List<Photo> updatedPhotos;

        public PhotoCreatedEventHandlerTest()
        {
            eagleEyeRepository = A.Fake<IEagleEyeRepository>();
            sut = new PhotoCreatedEventHandler(eagleEyeRepository);

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
            var initTimestamp = DateTimeOffset.UtcNow;
            var fileHash = new byte[32];
            var expectedPhoto = CreatePhoto(guid, version, filename, fileHash, initTimestamp, null, null);

            // act
            await sut.Handle(new PhotoCreated(guid, filename, fileMimeType, fileHash)
                {
                    TimeStamp = initTimestamp,
                });

            // assert
            A.CallTo(eagleEyeRepository).MustHaveHappenedOnceExactly();
            savedPhotos.Should().HaveCount(1);
            savedPhotos.Single().Should().BeEquivalentTo(expectedPhoto);
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
