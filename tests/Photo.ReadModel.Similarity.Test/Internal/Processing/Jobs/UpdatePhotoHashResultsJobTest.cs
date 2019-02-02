namespace Photo.ReadModel.Similarity.Test.Internal.Processing
{
    using System;

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
        private readonly DbSet<Scores> dbScores;

        private readonly Guid guid;
        private readonly int version;
        private readonly string hashIdentifierString;
        private ISimilarityJobConfiguration configuration;

        public UpdatePhotoHashResultsJobTest()
        {
            dbScores = A.Fake<DbSet<Scores>>();

            dbContext = A.Fake<ISimilarityDbContext>();
            A.CallTo(() => dbContext.Scores).Returns(dbScores);

            var contextFactory = A.Fake<ISimilarityDbContextFactory>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            repository = A.Fake<IInternalStatelessSimilarityRepository>();

            configuration = A.Fake<ISimilarityJobConfiguration>();
            A.CallTo(() => configuration.ThresholdPercentageSimilarityStorage).Returns(50);

            sut = new UpdatePhotoHashResultsJob(repository, contextFactory, configuration);

            guid = Guid.NewGuid();
            version = 3;
            hashIdentifierString = "sdf";
        }

/*
        [Fact]
        public void Execute_ShouldDoSomething()
        {
            // todo implement test.

            // arrange

            // act
            sut.Execute(guid, version, hashIdentifierString);

            // assert
            Assert.True(true);
        }
*/
    }
}
