namespace SearchEngine.LuceneNet.Core
{
    using Lucene.Net.Store;

    public class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}