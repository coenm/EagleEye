namespace Photo.ReadModel.SearchEngineLucene.Interface
{
    using Lucene.Net.Store;

    public interface ILuceneDirectoryFactory
    {
        Directory Create();
    }
}
