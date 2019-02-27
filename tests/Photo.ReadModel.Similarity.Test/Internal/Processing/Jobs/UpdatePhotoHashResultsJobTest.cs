namespace Photo.ReadModel.Similarity.Test.Internal.Processing.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class UpdatePhotoHashResultsJobTest
    {
        private readonly UpdatePhotoHashResultsJob sut;
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly ISimilarityDbContext dbContext;
        private readonly Guid photoGuid;
        private readonly int version;
        private readonly string hashIdentifierString;
        private readonly ISimilarityJobConfiguration configuration;
        private readonly HashIdentifiers hashIdentifier;
        private readonly List<PhotoHash> photoHashes;
        private readonly PhotoHash photoHash;

        public UpdatePhotoHashResultsJobTest()
        {
            photoGuid = Guid.NewGuid();
            version = 3;
            hashIdentifierString = "sdf";

            var dbScores = A.Fake<DbSet<Scores>>();

            dbContext = A.Fake<ISimilarityDbContext>();
            A.CallTo(() => dbContext.Scores).Returns(dbScores);

            var contextFactory = A.Fake<ISimilarityDbContextFactory>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            repository = A.Fake<IInternalStatelessSimilarityRepository>();

            configuration = A.Fake<ISimilarityJobConfiguration>();
            A.CallTo(() => configuration.ThresholdPercentageSimilarityStorage).Returns(50);

            hashIdentifier = new HashIdentifiers
            {
                HashIdentifier = hashIdentifierString,
                Id = 8,
            };
            A.CallTo(() => repository.GetHashIdentifier(dbContext, hashIdentifierString)).Returns(hashIdentifier);

            photoHash = new PhotoHash
            {
                Id = photoGuid,
                HashIdentifier = hashIdentifier,
                HashIdentifiersId = hashIdentifier.Id,
                Version = 1,
                Hash = 8,
            };
            A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, photoGuid, hashIdentifier)).Returns(photoHash);

            photoHashes = new List<PhotoHash>();
            A.CallTo(() => repository.GetPhotoHashesByHashIdentifier(dbContext, hashIdentifier)).Returns(photoHashes);

            sut = new UpdatePhotoHashResultsJob(repository, contextFactory, configuration);
        }

        [Fact]
        public void Execute_WhenNoHashIdentifierFound_ThenNothingIsProcessedOrSaved()
        {
            // arrange
            A.CallTo(() => repository.GetHashIdentifier(dbContext, hashIdentifierString)).Returns(null);

            // act
            sut.Execute(photoGuid, version, hashIdentifierString);

            // assert
            A.CallTo(() => repository.GetHashIdentifier(dbContext, hashIdentifierString)).MustHaveHappenedOnceExactly();
            A.CallTo(repository).MustHaveHappenedOnceExactly();
            AssertDataContextIsNotSaved();
        }

        [Fact]
        public void Execute_WhenNoPhotoHashIsFound_ThenNothingIsProcessedOrSaved()
        {
            // arrange
            A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, photoGuid, hashIdentifier)).Returns(null);

            // act
            sut.Execute(photoGuid, version, hashIdentifierString);

            // assert
            A.CallTo(() => repository.GetHashIdentifier(dbContext, hashIdentifierString)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => repository.GetPhotoHashByIdAndHashIdentifier(dbContext, photoGuid, hashIdentifier)).MustHaveHappenedOnceExactly());
            A.CallTo(repository).MustHaveHappenedTwiceExactly();
            AssertDataContextIsNotSaved();
        }

        [Fact]
        public void Execute_WhenNoOutdatedScoresAreFound_ThenNoScoresAreDeleted()
        {
            // arrange
            var emptyListOfScores = new List<Scores>();
            A.CallTo(() => repository.GetOutdatedScores(dbContext, photoGuid, hashIdentifier, version)).Returns(emptyListOfScores);

            // act
            sut.Execute(photoGuid, version, hashIdentifierString);

            // assert
            A.CallTo(() => repository.GetOutdatedScores(dbContext, photoGuid, hashIdentifier, version)).MustHaveHappenedOnceExactly();
            A.CallTo(() => repository.DeleteScores(dbContext, A<IEnumerable<Scores>>._)).MustNotHaveHappened();
        }

        private void AssertDataContextIsNotSaved()
        {
            A.CallTo(() => dbContext.SaveChanges()).MustNotHaveHappened();
            A.CallTo(() => dbContext.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }
    }
}
