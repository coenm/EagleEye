namespace EagleEye.Photo.ReadModel.Similarity.Internal.Processing.Jobs
{
    internal interface ISimilarityJobConfiguration
    {
        /// <summary>
        /// Percentage of similarity between images. Between 0% and 100%.
        /// </summary>
        double ThresholdPercentageSimilarityStorage { get; }
    }
}