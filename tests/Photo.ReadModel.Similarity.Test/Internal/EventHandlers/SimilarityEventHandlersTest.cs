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
        private const string HashAlgorithm1 = "hashAlgo1";
        private const string HashAlgorithm2 = "hashAlgo2";

        private readonly DbConnection connection;
        private readonly ISimilarityDbContextFactory contextFactory;
        private readonly IBackgroundJobClient hangFireClient;
        private readonly SimilarityEventHandlers sut;
        private readonly List<Job> jobsAdded;
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

            jobsAdded = new List<Job>();
            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._))
                .Invokes(call => jobsAdded.Add(call.Arguments[0] as Job));

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
            var hashValue = new byte[0];

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
            var hashValue = new byte[] { 0x12 };

            var alreadyExistingHashIdentifier = CreateHashIdentifiers(1, HashAlgorithm1);

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

        [Fact]
        public async Task Handle_PhotoHashCleared_WhenNothingIsInDatabase_ThenOnlyHashIdentifierAddedInDbAndHangFireJobCreated()
        {
            // arrange
            var guid = Guid.NewGuid();

            // act
            await sut.Handle(CreatePhotoHashClearedEvent(guid, HashAlgorithm1, Version, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().HaveCount(1, "because one item should have been added into an empty table.")
                    .And
                    .BeEquivalentTo(CreateHashIdentifiers(1, HashAlgorithm1));

                ctx.PhotoHashes.Should().BeEmpty();
                ctx.Scores.Should().BeEmpty();
            }

            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._)).MustHaveHappenedOnceExactly();
            AssertHangFireJobHasBeenCreated(typeof(ClearPhotoHashResultsJob), nameof(ClearPhotoHashResultsJob.Execute), guid, Version, HashAlgorithm1);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        public async Task Handle_PhotoHashCleared_WhenMultiplePhotoHashesAreInDatabase_ThenOnlyHashIdentifierAddedInDbAndHangFireJobCreated(int eventVersion, bool expectedItemRemovedFromDatabase)
        {
            // arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var hashIdentifier1 = CreateHashIdentifiers(1, HashAlgorithm1);
            var hashIdentifier2 = CreateHashIdentifiers(2, HashAlgorithm2);
            var photoHash11 = CreatePhotoHash(guid1, hashIdentifier1, new byte[1], 2);
            var photoHash12 = CreatePhotoHash(guid2, hashIdentifier1, new byte[2], 4);
            var photoHash21 = CreatePhotoHash(guid1, hashIdentifier2, new byte[3], 6);

            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.Add(hashIdentifier1);
                ctx.HashIdentifiers.Add(hashIdentifier2);
                ctx.PhotoHashes.Add(photoHash11);
                ctx.PhotoHashes.Add(photoHash12);
                ctx.PhotoHashes.Add(photoHash21);

                await ctx.SaveChangesAsync().ConfigureAwait(false);
            }

            // act
            await sut.Handle(CreatePhotoHashClearedEvent(guid1, HashAlgorithm1, eventVersion, timestamp), CancellationToken.None);

            // assert
            using (var ctx = contextFactory.CreateDbContext())
            {
                ctx.HashIdentifiers.ToList().Should().BeEquivalentTo(hashIdentifier1, hashIdentifier2);
                if (expectedItemRemovedFromDatabase)
                    ctx.PhotoHashes.Should().BeEquivalentTo(photoHash12, photoHash21);
                else
                    ctx.PhotoHashes.ToList().Should().BeEquivalentTo(photoHash11, photoHash12, photoHash21);
                ctx.Scores.Should().BeEmpty();
            }

            A.CallTo(() => hangFireClient.Create(A<Job>._, A<IState>._)).MustHaveHappenedOnceExactly();
            AssertHangFireJobHasBeenCreated(typeof(ClearPhotoHashResultsJob), nameof(ClearPhotoHashResultsJob.Execute), guid1, eventVersion, HashAlgorithm1);
        }

        [DebuggerStepThrough]
        private static PhotoHash CreatePhotoHash(Guid guid, HashIdentifiers hashIdentifier, byte[] hash, int version)
        {
            return new PhotoHash
            {
                Id = guid,
                HashIdentifier = hashIdentifier,
                Hash = hash,
                HashIdentifiersId = hashIdentifier.Id,
                Version = version,
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

        [DebuggerStepThrough]
        private static PhotoHashUpdated CreatePhotoHashUpdatedEvent(Guid guid, string hashAlgorithm, byte[] hashValue, int version, DateTimeOffset timestamp)
        {
            return new PhotoHashUpdated(guid, hashAlgorithm, hashValue)
            {
                Version = version,
                TimeStamp = timestamp,
            };
        }

        [DebuggerStepThrough]
        private static PhotoHashCleared CreatePhotoHashClearedEvent(Guid guid, string hashAlgorithm, int version, DateTimeOffset timestamp)
        {
            return new PhotoHashCleared(guid, hashAlgorithm)
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
