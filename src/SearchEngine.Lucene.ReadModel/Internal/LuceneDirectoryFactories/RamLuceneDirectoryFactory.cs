namespace SearchEngine.LuceneNet.ReadModel.Internal.LuceneDirectoryFactories
{
    using Lucene.Net.Store;
    using SearchEngine.LuceneNet.ReadModel.Interface;

    public class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}
