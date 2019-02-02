namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing
{
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;
    using Helpers.Guards;

    internal class StaticSimilarityJobConfiguration : ISimilarityJobConfiguration
    {
        public StaticSimilarityJobConfiguration(double thresholdPercentageSimilarityStorage)
        {
            Guard.MustBeBetweenOrEqualTo(thresholdPercentageSimilarityStorage, 0d, 100d, nameof(thresholdPercentageSimilarityStorage));

            ThresholdPercentageSimilarityStorage = thresholdPercentageSimilarityStorage;
        }

        public double ThresholdPercentageSimilarityStorage { get; }
    }
}
