namespace SearchEngine.LuceneNet.Core
{
    using Lucene.Net.Store;

    using Directory = Lucene.Net.Store.Directory;

    public class RamDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}