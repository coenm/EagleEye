namespace EagleEye.FileImporter.Similarity
{
    public class SimilarityResult
    {
        public byte[] OtherImageHash { get; set; }

        public double AverageHash { get; set; }

        public double DifferenceHash { get; set; }

        public double PerceptualHash { get; set; }
    }
}
