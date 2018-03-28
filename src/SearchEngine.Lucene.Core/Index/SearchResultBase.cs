namespace SearchEngine.LuceneNet.Core.Index
{
    public abstract class SearchResultBase
    {
        protected SearchResultBase(float score)
        {
            Score = score;
        }

        public float Score { get; }
    }
}