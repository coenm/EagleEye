namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.Domain.Events;
    using FakeItEasy;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Photo.ReadModel.Similarity.Internal.EventHandlers;
    using Photo.ReadModel.Similarity.Internal.Processing;
    using Xunit;

    public class SimilarityEventHandlersTest : IDisposable
    {
        private const int Version = 0;
        private readonly DbConnection connection;
        private readonly ISimilarityDbContextFactory contextFactory;
        private readonly IBackgroundJobClient hangFireClient;
        private readonly SimilarityEventHandlers sut;
        private readonly List<HangFireJobAdd> jobsAdded;
        private readonly DateTimeOffset timestamp;

        public SimilarityEventHandlersTest()
        {
            timestamp = DateTimeOffset.UtcNow;

            // In-memory database only exists while the connection is open
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SimilarityDbContext>()
                .UseSqlite(connection)
                .Options;

            contextFactory = new SimilarityDbContextFactory(options);

            contextFactory.Initialize().GetAwaiter().GetResult();

            hangFireClient = A.Fake<IBackgroundJobClient>();

            jobsAdded = new List<HangFireJobAdd>();
            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._))
                .Invokes(call => jobsAdded.Add(new HangFireJobAdd(call.Arguments[0] as Job, call.Arguments[1] as IState)));

            sut = new SimilarityEventHandlers(A.Dummy<ISimilarityRepository>(), contextFactory, hangFireClient);
        }

        public void Dispose()
        {
            connection.Close();
        }

        [Fact]
        public async Task Handle_PhotoHashUpdated_ShouldAddHashIdentifierAndPhotoHashIntoDbAndCreateHangFireJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            const string hashAlgorithm = "hashAlgo1";
            var hashValue = new byte[0];

            // act
            await sut.Handle(CreatePhotoHashUpdatedEvent(guid, hashAlgorithm, hashValue, Version, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().HaveCount(1, "because one item should have been added into an empty table.")
                    .And
                    .BeEquivalentTo(new HashIdentifiers
                        {
                            Id = 1,
                            HashIdentifier = hashAlgorithm,
                        });

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
            AssertHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, Version, hashAlgorithm);
        }

        [Fact]
        public async Task Handle_PhotoHashUpdated_ShouldUpdatePhotoHashesEntryWhenAlreadyExistAndCreateHangFireJob()
        {
            // arrange
            var guid = Guid.NewGuid();
            const string hashAlgorithm = "hashAlgo1";
            var hashValue = new byte[] { 0x12 };

            var alreadyExistingHashIdentifier = new HashIdentifiers
            {
                Id = 1,
                HashIdentifier = hashAlgorithm,
            };

            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.Add(alreadyExistingHashIdentifier);

                ctx.PhotoHashes.Add(new PhotoHash
                {
                    Id = guid,
                    HashIdentifier = alreadyExistingHashIdentifier,
                    Hash = new byte[0],
                    HashIdentifiersId = 1,
                    Version = Version,
                });

                await ctx.SaveChangesAsync();
            }

            // act
            await sut.Handle(CreatePhotoHashUpdatedEvent(guid, hashAlgorithm, hashValue, 2, timestamp), CancellationToken.None);

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
            AssertHangFireJobHasBeenCreated(typeof(UpdatePhotoHashResultsJob), nameof(UpdatePhotoHashResultsJob.Execute), guid, 2, hashAlgorithm);
        }

        [DebuggerStepThrough]
        private static PhotoHashUpdated CreatePhotoHashUpdatedEvent(Guid guid, string hashAlgorithm, byte[] hashValue, int version, DateTimeOffset timestamp)
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
                    item.Job.Type == type
                    &&
                    item.Job.Method.Name == methodName)
                .Which
                .Job.Args.Should().BeEquivalentTo(parameters);
        }

        private struct HangFireJobAdd
        {
            public HangFireJobAdd(Job job, IState state)
            {
                Job = job;
                State = state;
            }

            public Job Job { get; }

            public IState State { get; }
        }
    }
}
