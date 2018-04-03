namespace SearchEngine.Interface.Queries
{
    /// <summary>Gets order information of a single order by its id.</summary>
    public class WildcardSearchQuery : ISearchEngineQuery<SearchResult>
    {
        /// <summary>The id of the order to get.</summary>
        public string Query { get; set; }
    }
}