namespace Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories
{
    using Lucene.Net.Store;
    using Photo.ReadModel.SearchEngineLucene.Interface;

    public class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}
