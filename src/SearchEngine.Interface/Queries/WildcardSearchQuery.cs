namespace SearchEngine.Interface.Queries
{
    public class WildcardSearchQuery : ISearchEngineQuery<SearchResult>
    {
        public string Query { get; set; }
    }
}