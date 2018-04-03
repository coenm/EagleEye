namespace SearchEngine.Interface.Queries
{
    public class SearchResult
    {
        public int Count { get; set; }

        public SearchItem[] Items { get; set; }
    }
}