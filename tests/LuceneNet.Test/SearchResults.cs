namespace EagleEye.LuceneNet.Test
{
    internal class SearchResults<T> where T : class, new()
    {
        public SearchResults(T data, float score)
        {
            Data = data;
            Score = score;
        }

        public T Data { get; }
        public float Score { get; }
    }
}