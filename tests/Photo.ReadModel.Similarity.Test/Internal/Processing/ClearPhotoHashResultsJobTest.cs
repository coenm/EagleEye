namespace Photo.ReadModel.Similarity.Test.Internal.Processing
{
    using System;
    using System.Collections.Generic;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing;
    using FakeItEasy;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class ClearPhotoHashResultsJobTest
    {
        private readonly ClearPhotoHashResultsJob sut;
        private readonly IInternalStatelessSimilarityRepository repository;
        private readonly ISimilarityDbContext dbContext;
        private readonly DbSet<Scores> dbScores;

        private readonly Guid guid;
        private readonly int version;
        private readonly string hashIdentifierString;

        public ClearPhotoHashResultsJobTest()
        {
            dbScores = A.Fake<DbSet<Scores>>();

            dbContext = A.Fake<ISimilarityDbContext>();
            A.CallTo(() => dbContext.Scores).Returns(dbScores);

            var contextFactory = A.Fake<ISimilarityDbContextFactory>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            repository = A.Fake<IInternalStatelessSimilarityRepository>();

            sut = new ClearPhotoHashResultsJob(repository, contextFactory);

            guid = Guid.NewGuid();
            version = 3;
            hashIdentifierString = "sdf";
        }

        [Fact]
        public void Execute_ShouldRemoveScoresFromDatabaseContext()
        {
            // arrange
            var hashIdentifier = new HashIdentifiers
            {
                HashIdentifier = hashIdentifierString,
                Id = 23,
            };
            var scores = new List<Scores>
            {
                new Scores(),
            };

            A.CallTo(() => repository.GetOrAddHashIdentifier(dbContext, hashIdentifierString))
                .Returns(hashIdentifier);
            A.CallTo(() => repository.GetHashScoresByIdAndBeforeVersion(dbContext, hashIdentifier.Id, guid, version))
                .Returns(scores);

            // act
            sut.Execute(guid, version, hashIdentifierString);

            // assert
            A.CallTo(() => dbScores.RemoveRange(scores)).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => dbContext.SaveChanges()).MustHaveHappenedOnceExactly())
                .Then(A.CallTo(() => dbContext.Dispose()).MustHaveHappenedOnceExactly());
        }
    }
}
