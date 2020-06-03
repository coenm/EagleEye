namespace Photo.ReadModel.Similarity.Test.Internal.Processing.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using FakeItEasy;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class UpdatePhotoHashResultsJobTest
    {
        private const string HashIdentifierString = "sdf";
        private static readonly Guid PhotoGuid = new Guid("671054EF-925B-4197-9B91-F50AD5767A40");
        private static readonly HashIdentifiers HashIdentifier = new HashIdentifiers
                                                                 {
                                                                     HashIdentifier = HashIdentifierString,
                                                                     Id = 8,
                                                                 };

        private readonly UpdatePhotoHashResultsJob sut;
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly int version;
        private readonly ISimilarityDbContext dbContext;
        private readonly List<PhotoHash> photoHashes;
        private readonly PhotoHash photoHash;
        private readonly List<Scores> addedScores;
        private readonly ISimilarityJobConfiguration configuration;

        public UpdatePhotoHashResultsJobTest()
        {
            version = 3;

            var dbScores = A.Fake<DbSet<Scores>>();

            dbContext = A.Fake<ISimilarityDbContext>();
            A.CallTo(() => dbContext.Scores).Returns(dbScores);

            var contextFactory = A.Fake<ISimilarityDbContextFactory>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            repository = A.Fake<IInternalStatelessSimilarityRepository>();

            configuration = A.Fake<ISimilarityJobConfiguration>();
            A.CallTo(() => configuration.ThresholdPercentageSimilarityStorage).Returns(50);

            A.CallTo(() => repository.GetHashIdentifier(dbContext, HashIdentifierString)).Returns(HashIdentifier);

            photoHash = new PhotoHash
                {
                    Id = PhotoGuid,
                    HashIdentifier = HashIdentifier,
                    HashIdentifiersId = HashIdentifier.Id,
                    Version = 1,
                    Hash = 8,
                };
            A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, PhotoGuid, HashIdentifier)).Returns(photoHash);

            addedScores = new List<Scores>();
            A.CallTo(() => dbContext.Scores.Add(A<Scores>._)).Invokes(call => addedScores.Add(call.Arguments[0] as Scores));

            photoHashes = new List<PhotoHash>();
            A.CallTo(() => repository.GetPhotoHashesByHashIdentifier(dbContext, HashIdentifier)).Returns(photoHashes);

            sut = new UpdatePhotoHashResultsJob(repository, contextFactory, configuration);
        }

        [Fact]
        public void Execute_WhenNoHashIdentifierFound_ThenNothingIsProcessedOrSaved()
        {
            // arrange
            A.CallTo(() => repository.GetHashIdentifier(dbContext, HashIdentifierString)).Returns(null);

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            A.CallTo(() => repository.GetHashIdentifier(dbContext, HashIdentifierString)).MustHaveHappenedOnceExactly();
            A.CallTo(repository).MustHaveHappenedOnceExactly();
            AssertDataContextIsNotSaved();
        }

        [Fact]
        public void Execute_WhenNoPhotoHashIsFound_ThenNothingIsProcessedOrSaved()
        {
            // arrange
            A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, PhotoGuid, HashIdentifier)).Returns(null);

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            A.CallTo(() => repository.GetHashIdentifier(dbContext, HashIdentifierString)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, PhotoGuid, HashIdentifier)).MustHaveHappenedOnceExactly());
            A.CallTo(repository).MustHaveHappenedTwiceExactly();
            AssertDataContextIsNotSaved();
        }

        [Fact]
        public void Execute_WhenNoOutdatedScoresAreFound_ThenNoScoresAreDeleted()
        {
            // arrange
            var emptyListOfScores = new List<Scores>();
            A.CallTo(() => repository.GetOutdatedScores(dbContext, PhotoGuid, HashIdentifier, version)).Returns(emptyListOfScores);

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            A.CallTo(() => repository.GetOutdatedScores(dbContext, PhotoGuid, HashIdentifier, version)).MustHaveHappenedOnceExactly();
            A.CallTo(() => repository.DeleteScores(dbContext, A<IEnumerable<Scores>>._)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_WhenOutdatedScoresAreFound_ThenScoresAreDeleted()
        {
            // arrange
            var currentScores = new List<Scores>
                                {
                                    new Scores(),
                                    new Scores(),
                                };
            A.CallTo(() => repository.GetOutdatedScores(dbContext, PhotoGuid, HashIdentifier, version)).Returns(currentScores);

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            A.CallTo(() => repository.DeleteScores(dbContext, currentScores)).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [MemberData(nameof(DatabaseGuidAndExpectedScore))]
        internal void Execute_ShouldProcessEachHashInDatabase(Guid databasePhotoGuid, Scores expectedScore)
        {
            // arrange
            AddPhotoToDatabase(
                               new PhotoHash
                               {
                                   Hash = 3,
                                   Version = 5,
                                   Id = databasePhotoGuid,
                                   HashIdentifier = HashIdentifier,
                                   HashIdentifiersId = HashIdentifier.Id,
                               });

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            addedScores.Should().BeEquivalentTo(expectedScore);
            A.CallTo(() => dbContext.Scores.Add(A<Scores>._)).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => dbContext.SaveChanges()).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => dbContext.Dispose()).MustHaveHappenedOnceExactly());
        }

        [Fact]
        internal void Execute_ShouldNotAddScore_WhenScoreIsBelowThreshold()
        {
            // arrange
            A.CallTo(() => configuration.ThresholdPercentageSimilarityStorage).Returns(99);
            AddPhotoToDatabase(
                               new PhotoHash
                               {
                                   Hash = 3,
                                   Version = 5,
                                   Id = Guid.NewGuid(),
                                   HashIdentifier = HashIdentifier,
                                   HashIdentifiersId = HashIdentifier.Id,
                               });

            // act
            sut.Execute(PhotoGuid, version, HashIdentifierString);

            // assert
            A.CallTo(() => dbContext.Scores.Add(A<Scores>._)).MustNotHaveHappened(); // because the calculated score will be 95.3125 (which is < 99)
            A.CallTo(() => dbContext.SaveChanges()).MustHaveHappenedOnceExactly();
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Required for xUnits MemberData attribute.")]
        public static IEnumerable<object[]> DatabaseGuidAndExpectedScore()
        {
            // this guid is less than photoGuid so it will travel a specific path.
            var databaseGuid = new Guid("18222BEA-A15D-44A9-A807-9A445D3173FC");
            yield return new object[]
                         {
                             databaseGuid,
                             new Scores
                             {
                                 HashIdentifier = HashIdentifier,
                                 Score = 95.3125,
                                 VersionPhotoA = 5,
                                 VersionPhotoB = 3,
                                 PhotoA = databaseGuid,
                                 PhotoB = PhotoGuid,
                             },
                         };

            // this guid is greater than photoGuid so it will travel a specific path.
            databaseGuid = new Guid("91E042BA-008C-43A1-981D-2269CF8DB84A");
            yield return new object[]
                         {
                             databaseGuid,
                             new Scores
                             {
                                 HashIdentifier = HashIdentifier,
                                 Score = 95.3125,
                                 VersionPhotoA = 3,
                                 VersionPhotoB = 5,
                                 PhotoA = PhotoGuid,
                                 PhotoB = databaseGuid,
                             },
                         };
        }

        private void AddPhotoToDatabase(PhotoHash item) => photoHashes.Add(item);

        private void AssertDataContextIsNotSaved()
        {
            A.CallTo(() => dbContext.SaveChanges()).MustNotHaveHappened();
            A.CallTo(() => dbContext.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
