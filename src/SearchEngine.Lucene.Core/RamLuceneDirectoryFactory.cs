namespace SearchEngine.LuceneNet.Core
{
    using Lucene.Net.Store;

    using Directory = Lucene.Net.Store.Directory;

    public class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}