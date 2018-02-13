using System.Collections.Generic;

namespace EagleEye.FileImporter.Infrastructure.JsonSimilarity
{
    public class SimilarityResultStorage
    {
        public SimilarityResultStorage()
        {
            ImageHash = new List<byte[]>(2);
        }

        public List<byte[]> ImageHash { get; set; }

        public double AverageHash { get; set; }

        public double DifferenceHash { get; set; }

        public double PerceptualHash { get; set; }
    }
}