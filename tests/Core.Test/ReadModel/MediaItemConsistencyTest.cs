namespace EagleEye.Core.Test.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.Domain.Events;
    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;
    using EagleEye.Core.ReadModel.EventHandlers;

    using FluentAssertions;

    using Newtonsoft.Json;

    using Xunit;

    public class MediaItemConsistencyTest
    {
        private readonly MediaItemConsistency sut;
        private readonly IMediaItemRepository mediaItemRepository;

        public MediaItemConsistencyTest()
        {
            IMediaItemDbContextFactory mediaItemDbContextFactory = new ExploringEntityFrameworkTests.InMemoryMediaItemDbContextFactory();
            mediaItemRepository = new EntityFrameworkMediaItemRepository(mediaItemDbContextFactory);
            sut = new MediaItemConsistency(mediaItemRepository);
        }

        [Fact]
        public async Task HandleMediaItemCreated_ShouldSaveDataToRepositoryTest()
        {
            // arrange
            var guid = Guid.NewGuid();
            var initialTags = new[] { "soccer", "sports" };
            var initialPersons = new[] { "alice", "bob" };
            var initTimestamp = DateTimeOffset.UtcNow;

            // act
            await sut.Handle(new MediaItemCreated(guid, "FAKE NAME1", initialTags, initialPersons)
                                  {
                                      TimeStamp = initTimestamp
                                  }).ConfigureAwait(false);

            // assert
            var result = await mediaItemRepository.GetByIdAsync(guid).ConfigureAwait(false);
            result.Should().BeEquivalentTo(new MediaItemDb
                                               {
                                                   Id = guid,
                                                   Version = 0,
                                                   Filename = "FAKE NAME1",
                                                   TimeStampUtc = initTimestamp,
                                                   SerializedMediaItemDto = JsonConvert.SerializeObject(new MediaItemDto
                                                                                                            {
                                                                                                                Tags = new List<string>(initialTags),
                                                                                                                Persons = new List<string>(initialPersons)
                                                                                                            })
                                               });
        }


        [Fact]
        public async Task HandleEvents_ShouldSaveDataToRepositoryTest()
        {
            // arrange
            var guid = Guid.NewGuid();
            var initialTags = new[] { "soccer", "sports" };
            var initialPersons = new[] { "alice", "bob" };
            var initTimestamp = DateTimeOffset.UtcNow;

            // act
            await sut.Handle(new MediaItemCreated(guid, "FAKE NAME1", initialTags, initialPersons)
                                  {
                                      Version = 1,
                                      TimeStamp = initTimestamp
                                  });
            await sut.Handle(new PersonsAddedToMediaItem(guid, "Calvin", "Darion", "Eve")
                                  {
                                      Version = 2,
                                      TimeStamp = initTimestamp.AddHours(2)
            });
            await sut.Handle(new PersonsRemovedFromMediaItem(guid, "Darion", "alice")
                                  {
                                      Version = 3,
                                      TimeStamp = initTimestamp.AddHours(3)
                                  });

            // assert
            var result = await mediaItemRepository.GetByIdAsync(guid).ConfigureAwait(false);
            result.Should().BeEquivalentTo(new MediaItemDb
                                               {
                                                   Id = guid,
                                                   Version = 3,
                                                   Filename = "FAKE NAME1",
                                                   TimeStampUtc = initTimestamp.AddHours(3),
                                                   SerializedMediaItemDto = JsonConvert.SerializeObject(new MediaItemDto
                                                                                                            {
                                                                                                                Tags = new List<string>(initialTags),
                                                                                                                Persons = new List<string>
                                                                                                                              {
                                                                                                                                  "bob",
                                                                                                                                  "Calvin",
                                                                                                                                  "Eve"
                                                                                                                              }
                                                                                                            })
                                               });
        }
    }
}