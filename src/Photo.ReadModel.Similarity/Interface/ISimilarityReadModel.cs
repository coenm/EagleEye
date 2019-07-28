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
        /// <summary>
        /// Returns all hash algorithms used for calculating hashes and similarities.
        /// </summary>
        /// <returns>List of hash algorithms.</returns>
        [NotNull] Task<List<string>> GetHashAlgorithmsAsync();

        /// <summary>
        /// Count all photo's that are similar to the photo with <paramref name="photoGuid"/> using <paramref name="hashAlgorithm"/> and respecting the <paramref name="scoreThreshold"/>.
        /// </summary>
        /// <param name="photoGuid">Photo id of the photo to find similar matches to.</param>
        /// <param name="hashAlgorithm">HashAlgorithm used to calculate the image hash. See <see cref="GetHashAlgorithmsAsync"/> to get a list of algorithms..</param>
        /// <param name="scoreThreshold">threshold for the similarity (percentage).</param>
        /// <returns>The number of found matches.</returns>
        Task<int> CountSimilaritiesAsync(Guid photoGuid, [NotNull] string hashAlgorithm, double scoreThreshold);

        /// <summary>
        /// Count all photo's that are similar to the photo with <paramref name="photoGuid"/> using <paramref name="hashAlgorithm"/> and respecting the <paramref name="scoreThreshold"/>.
        /// </summary>
        /// <param name="photoGuid">Photo id of the photo to find similar matches to.</param>
        /// <param name="hashAlgorithm">HashAlgorithm used to calculate the image hash. See <see cref="GetHashAlgorithmsAsync"/> to get a list of algorithms.</param>
        /// <param name="scoreThreshold">threshold for the similarity (percentage).</param>
        /// <returns><see cref="SimilarityResultSet"/> containing the found photo ids together with some metadata.</returns>
        [NotNull] Task<SimilarityResultSet> GetSimilaritiesAsync(Guid photoGuid, [NotNull] string hashAlgorithm, float scoreThreshold);
    }
}
