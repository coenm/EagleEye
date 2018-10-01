namespace SearchEngine.LuceneNet.Core
{
    using Lucene.Net.Store;

    public interface ILuceneDirectoryFactory
    {
        Directory Create();
    }
}
