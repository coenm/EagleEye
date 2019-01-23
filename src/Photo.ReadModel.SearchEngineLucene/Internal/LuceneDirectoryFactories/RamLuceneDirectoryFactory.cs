namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories
{
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using Lucene.Net.Store;

    public class RamLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        public Directory Create()
        {
            return new RAMDirectory();
        }
    }
}
