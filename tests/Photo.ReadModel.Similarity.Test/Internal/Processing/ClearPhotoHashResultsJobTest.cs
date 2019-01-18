namespace Photo.ReadModel.Similarity.Test.Internal.Processing
{
    using System;

    using FakeItEasy;
    using Photo.ReadModel.Similarity.Internal.EntityFramework;
    using Photo.ReadModel.Similarity.Internal.Processing;
    using Photo.ReadModel.Similarity.Test.Mocks;
    using Xunit;

    public class ClearPhotoHashResultsJobTest : IDisposable
    {
        private readonly InMemorySimilarityDbContextFactory contextFactory;
        private readonly ClearPhotoHashResultsJob sut;

        public ClearPhotoHashResultsJobTest()
        {
            contextFactory = new InMemorySimilarityDbContextFactory();
            contextFactory.Initialize().GetAwaiter().GetResult();

            sut = new ClearPhotoHashResultsJob(A.Dummy<ISimilarityRepository>(), contextFactory);
        }

        public void Dispose() => contextFactory.Dispose();
    }
}
