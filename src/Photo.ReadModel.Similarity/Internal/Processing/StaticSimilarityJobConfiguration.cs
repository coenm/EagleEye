namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing
{
    using Dawn;
    using EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs;

    internal class StaticSimilarityJobConfiguration : ISimilarityJobConfiguration
    {
        public StaticSimilarityJobConfiguration(double thresholdPercentageSimilarityStorage)
        {
            Dawn.Guard.Argument(thresholdPercentageSimilarityStorage, nameof(thresholdPercentageSimilarityStorage)).InRange(0d, 100d);

            ThresholdPercentageSimilarityStorage = thresholdPercentageSimilarityStorage;
        }

        public double ThresholdPercentageSimilarityStorage { get; }
    }
}
