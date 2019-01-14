namespace Photo.ReadModel.Similarity.Test.Internal.EventHandlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EagleEye.Photo.Domain.Events;
    using FakeItEasy;
    using Hangfire;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.EventHandlers;
    using Xunit;

    public class SimilarityEventHandlersTest
    {
        [Fact]
        public async Task This_Test_Will_Fail_Right_Now_WIP()
        {
            // see this
            // https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/in-memory

            // arrange
            var guid = Guid.NewGuid();
            var repository = A.Fake<ISimilarityRepository>();
            var contextFactory = A.Fake<ISimilarityDbContextFactory>();
            var hangFireClient = A.Fake<IBackgroundJobClient>();
            var dbContext = A.Fake<SimilarityDbContext>();
            A.CallTo(() => contextFactory.CreateDbContext()).Returns(dbContext);

            var sut = new SimilarityEventHandlers(repository, contextFactory, hangFireClient);

            // act
            await sut.Handle(new PhotoHashUpdated(guid, "hashAlgo1", new byte[0]), CancellationToken.None);

            // assert
        }
    }
}
