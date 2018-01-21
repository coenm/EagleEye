namespace FileImporter.Similarity
{
    using System.Collections.Generic;

    public interface ISimilarityRepository
    {
        IEnumerable<byte[]> FindAllRecordedMatches(byte[] contentHash);

        IEnumerable<SimilarityResult> FindSimilar(byte[] contentHash, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0);

        int CountSimilar(byte[] contentHash, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95);

        void Delete(byte[] contentHash);

        void AddOrUpdate(byte[] contentHash, SimilarityResult similarity);
    }
}