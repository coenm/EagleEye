namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing
{
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using Helpers.Guards; using Dawn;

    internal class StaticSimilarityJobConfiguration : ISimilarityJobConfiguration
    {
        public StaticSimilarityJobConfiguration(double thresholdPercentageSimilarityStorage)
        {
            Helpers.Guards.Guard.MustBeBetweenOrEqualTo(thresholdPercentageSimilarityStorage, 0d, 100d, nameof(thresholdPercentageSimilarityStorage));

            ThresholdPercentageSimilarityStorage = thresholdPercentageSimilarityStorage;
        }

        public double ThresholdPercentageSimilarityStorage { get; }
    }
}
