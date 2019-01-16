namespace Photo.ReadModel.SearchEngineLucene.Interface
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    [PublicAPI]
    public interface IReadModel
    {
        /// <summary>
        /// Search for full photo data.
        /// </summary>
        /// <param name="query">Search query. Cannot be null or empty.</param>
        /// <returns>A list of photo data matching the search criteria.</returns>
        [NotNull]
        List<Model.PhotoResult> FullSearch([NotNull] string query);

        /// <summary>
        /// Search for photo guids only.
        /// </summary>
        /// <param name="query">search query. Cannot be null or empty.</param>
        /// <returns>A list of guids matching the search criteria.</returns>
        [NotNull]
        List<Model.PhotoIdResult> Search([NotNull] string query);

        /// <summary>
        /// Count the number of photos that match the query.
        /// </summary>
        /// <param name="query">search query. Cannot be null or empty.</param>
        /// <returns>the number of photos found.</returns>
        int Count([NotNull] string query);
    }
}
