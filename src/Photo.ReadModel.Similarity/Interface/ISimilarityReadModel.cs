namespace EagleEye.Photo.ReadModel.Similarity.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.Similarity.Interface.Model;
    using JetBrains.Annotations;

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
