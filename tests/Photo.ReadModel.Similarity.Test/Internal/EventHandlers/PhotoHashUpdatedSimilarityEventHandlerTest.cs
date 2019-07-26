namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
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
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class PhotoHashUpdatedSimilarityEventHandlerTest : IDisposable
    {
        private const int Version = 0;
        private const string HashAlgorithm1 = "hashAlgo1";
        private const string HashAlgorithm2 = "hashAlgo2";

        private readonly InMemorySimilarityDbContextFactory contextFactory;
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly IBackgroundJobClient hangFireClient;
        private readonly PhotoHashUpdatedSimilarityEventHandler sut;
        private readonly List<Job> jobsAdded;
        private readonly DateTimeOffset timestamp;

        public PhotoHashUpdatedSimilarityEventHandlerTest()
        {
            timestamp = DateTimeOffset.UtcNow;

            contextFactory = new InMemorySimilarityDbContextFactory();
            contextFactory.Initialize().GetAwaiter().GetResult();

            repository = A.Fake<InternalSimilarityRepository>();

            hangFireClient = A.Fake<IBackgroundJobClient>();

            jobsAdded = new List<Job>();
            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._))
                .Invokes(call => jobsAdded.Add(call.Arguments[0] as Job));

            sut = new PhotoHashUpdatedSimilarityEventHandler(repository, contextFactory, hangFireClient);
        }

        public void Dispose() => contextFactory.Dispose();

        [Fact]
        public async Task Handle_PhotoHashUpdated_ShouldAddHashIdentifierAndPhotoHashIntoDbAndCreateHangFireJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            ulong hashValue = 16;

            // act
            await sut.Handle(CreatePhotoHashUpdatedEvent(guid, HashAlgorithm1, hashValue, Version, timestamp), CancellationToken.None);

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

            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._)).MustHaveHappenedOnceExactly();
            AssertHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, Version, HashAlgorithm1);
        }

        [Fact]
        public async Task Handle_PhotoHashUpdated_ShouldUpdatePhotoHashesEntryWhenAlreadyExistAndCreateHangFireJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            var hashValue = 12UL;

            var alreadyExistingHashIdentifier = CreateHashIdentifiers(1, HashAlgorithm1);

            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.Add(alreadyExistingHashIdentifier);

                ctx.PhotoHashes.Add(new PhotoHash
                {
                    Id = guid,
                    HashIdentifier = alreadyExistingHashIdentifier,
                    Hash = 0,
                    HashIdentifiersId = 1,
                    Version = Version,
                });

                await ctx.SaveChangesAsync();
            }

            // act
            await sut.Handle(CreatePhotoHashUpdatedEvent(guid, HashAlgorithm1, hashValue, 2, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().HaveCount(1)
                    .And
                    .BeEquivalentTo(alreadyExistingHashIdentifier);

                ctx.PhotoHashes.ToList().Should().HaveCount(1, "because the only and original item was just updated.")
                    .And
                    .BeEquivalentTo(new PhotoHash
                    {
                        Id = guid,
                        HashIdentifier = ctx.HashIdentifiers.Single(),
                        Hash = hashValue,
                        HashIdentifiersId = 1,
                        Version = 2,
                    });

                ctx.Scores.Should().BeEmpty();
            }

            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._)).MustHaveHappenedOnceExactly();
            AssertHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, 2, HashAlgorithm1);
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

        [DebuggerStepThrough]
        private static PhotoHashUpdated CreatePhotoHashUpdatedEvent(Guid guid, string hashAlgorithm, ulong hashValue, int version, DateTimeOffset timestamp)
        {
            return new PhotoHashUpdated(guid, hashAlgorithm, hashValue)
            {
                Version = version,
                TimeStamp = timestamp,
            };
        }

        private void AssertHangFireJobHasBeenCreated(Type type, string methodName, params object[] parameters)
        {
            jobsAdded.Should().Contain(item =>
                    item.Type == type
                    &&
                    item.Method.Name == methodName)
                .Which.Args.Should().BeEquivalentTo(parameters);
        }
    }
}
