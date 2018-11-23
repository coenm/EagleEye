namespace SearchEngine.LuceneNet.ReadModel.Interface
{
    using Lucene.Net.Store;

    public interface ILuceneDirectoryFactory
    {
        Directory Create();
    }
}
