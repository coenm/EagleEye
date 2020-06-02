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

    public class PhotoHashClearedSimilarityEventHandlerTest : IAsyncLifetime, IDisposable
    {
        private const int Version = 0;
        private const string HashAlgorithm1 = "hashAlgo1";
        private const string HashAlgorithm2 = "hashAlgo2";

        private readonly InMemorySimilarityDbContextFactory contextFactory;
        private readonly PhotoHashClearedSimilarityEventHandler sut;
        private readonly HangFireTestHelper hangFireTestHelper;
        private readonly DateTimeOffset timestamp;

        public PhotoHashClearedSimilarityEventHandlerTest()
        {
            timestamp = DateTimeOffset.UtcNow;
            contextFactory = new InMemorySimilarityDbContextFactory();
            IInternalStatelessSimilarityRepository repository = A.Fake<InternalSimilarityRepository>();
            hangFireTestHelper = new HangFireTestHelper();
            sut = new PhotoHashClearedSimilarityEventHandler(repository, contextFactory, hangFireTestHelper.HangFireClient);
        }

        public async Task InitializeAsync()
        {
            await contextFactory.Initialize();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public void Dispose() => contextFactory.Dispose();

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

            hangFireTestHelper.AssertSingleHangFireJobHasBeenCreated(typeof(ClearPhotoHashResultsJob), nameof(ClearPhotoHashResultsJob.Execute), guid, Version, HashAlgorithm1);
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
            var photoHash11 = CreatePhotoHash(guid1, hashIdentifier1, 1, 2);
            var photoHash12 = CreatePhotoHash(guid2, hashIdentifier1, 2, 4);
            var photoHash21 = CreatePhotoHash(guid1, hashIdentifier2, 3, 6);

            using (var ctx = contextFactory.CreateDbContext())
            {
                await ctx.HashIdentifiers.AddAsync(hashIdentifier1);
                await ctx.HashIdentifiers.AddAsync(hashIdentifier2);
                await ctx.PhotoHashes.AddAsync(photoHash11);
                await ctx.PhotoHashes.AddAsync(photoHash12);
                await ctx.PhotoHashes.AddAsync(photoHash21);

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

            hangFireTestHelper.AssertSingleHangFireJobHasBeenCreated(typeof(ClearPhotoHashResultsJob), nameof(ClearPhotoHashResultsJob.Execute), guid1, eventVersion, HashAlgorithm1);
        }

        [DebuggerStepThrough]
        private static PhotoHash CreatePhotoHash(Guid guid, HashIdentifiers hashIdentifier, ulong hash, int version)
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
        private static PhotoHashCleared CreatePhotoHashClearedEvent(Guid guid, string hashAlgorithm, int version, DateTimeOffset timestamp)
        {
            return new PhotoHashCleared(guid, hashAlgorithm)
            {
                Version = version,
                TimeStamp = timestamp,
            };
        }
    }
}
