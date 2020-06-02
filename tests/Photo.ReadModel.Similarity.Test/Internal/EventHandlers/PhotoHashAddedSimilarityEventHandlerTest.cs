namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EventHandlers;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using FakeItEasy;
    using FluentAssertions;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class PhotoHashAddedSimilarityEventHandlerTest : IAsyncLifetime, IDisposable
    {
        private const int Version = 0;
        private const string HashAlgorithm1 = "hashAlgo1";
        private readonly InMemorySimilarityDbContextFactory contextFactory;
        private readonly PhotoHashAddedSimilarityEventHandler sut;
        private readonly HangFireTestHelper hangFireTestHelper;
        private readonly DateTimeOffset timestamp;

        public PhotoHashAddedSimilarityEventHandlerTest()
        {
            timestamp = DateTimeOffset.UtcNow;
            contextFactory = new InMemorySimilarityDbContextFactory();
            IInternalStatelessSimilarityRepository repository = A.Fake<InternalSimilarityRepository>();
            hangFireTestHelper = new HangFireTestHelper();
            sut = new PhotoHashAddedSimilarityEventHandler(repository, contextFactory, hangFireTestHelper.HangFireClient);
        }

        public async Task InitializeAsync()
        {
            await contextFactory.Initialize();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public void Dispose() => contextFactory.Dispose();

        [Fact]
        public async Task Handle_PhotoHashAdded_ShouldAddToDatabaseAndScheduleJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            const ulong hashValue = 16UL;

            // act
            await sut.Handle(CreatePhotoHashAddedEvent(guid, HashAlgorithm1, hashValue, Version, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().HaveCount(1, "because one item should have been added into an empty table.")
                   .And.BeEquivalentTo(CreateHashIdentifiers(1, HashAlgorithm1));

                ctx.PhotoHashes.ToList().Should().HaveCount(1)
                   .And.BeEquivalentTo(new PhotoHash
                                       {
                                           Id = guid,
                                           HashIdentifier = ctx.HashIdentifiers.Single(),
                                           Hash = hashValue,
                                           HashIdentifiersId = 1,
                                           Version = Version,
                                       });

                ctx.Scores.Should().BeEmpty();
            }

            hangFireTestHelper.AssertSingleHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, Version, HashAlgorithm1);
        }

        [Fact]
        public async Task Handle_PhotoHashAdded_ShouldUpdateDbAndCreateHangFireJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            const ulong hashValue = 16UL;

            // act
            await sut.Handle(CreatePhotoHashAddedEvent(guid, HashAlgorithm1, hashValue, Version, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().HaveCount(1, "because one item should have been added into an empty table.")
                   .And
                   .BeEquivalentTo(CreateHashIdentifiers(1, HashAlgorithm1));

                ctx.PhotoHashes.ToList().Should().HaveCount(1)
                   .And
                   .BeEquivalentTo(new PhotoHash
                                   {
                                       Id = guid,
                                       HashIdentifier = ctx.HashIdentifiers.Single(),
                                       Hash = hashValue,
                                       HashIdentifiersId = 1,
                                       Version = Version,
                                   });

                ctx.Scores.Should().BeEmpty();
            }

            hangFireTestHelper.AssertSingleHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, Version, HashAlgorithm1);
        }

        [DebuggerStepThrough]
        private static PhotoHashAdded CreatePhotoHashAddedEvent(Guid id, string hashAlgorithm, in ulong hashValue, in int version, in DateTimeOffset dateTimeOffset)
        {
            return new PhotoHashAdded(id, hashAlgorithm, hashValue)
                   {
                       Version = version,
                       TimeStamp = dateTimeOffset,
                   };
        }

        [DebuggerStepThrough]
        private static HashIdentifiers CreateHashIdentifiers(int id, string hashIdentifier)
        {
            return new HashIdentifiers
                   {
                       Id = id,
                       HashIdentifier = hashIdentifier,
                   };
        }
    }
}
