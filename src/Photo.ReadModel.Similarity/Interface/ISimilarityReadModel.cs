namespace Photo.ReadModel.Similarity.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Photo.ReadModel.Similarity.Interface.Model;

    [PublicAPI]
    public interface ISimilarityReadModel
    {
        [ItemCanBeNull]
        Task<IEnumerable<SimilarityResult>> GetAllSimilaritiesAsync();

        Task<int> CountAllDistinctPhotosAsync();

        Task<int> CountAllSimilaritiesAsync();

        [ItemCanBeNull]
        Task<IEnumerable<string>> GetAllUsedHashIdentifiers();

        [CanBeNull]
        [ItemCanBeNull]
        Task<IEnumerable<SimilarityResult>> GetAllSimilaritiesAsync(Guid id, string hashIdentifier, float score);

        Task<int> CountAllSimilaritiesAsync(Guid id, string hashIdentifier, float score);
    }
}
