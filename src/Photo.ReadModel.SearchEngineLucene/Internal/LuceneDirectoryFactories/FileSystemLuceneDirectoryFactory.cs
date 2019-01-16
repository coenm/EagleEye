namespace Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories
{
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Lucene.Net.Store;
    using Photo.ReadModel.SearchEngineLucene.Interface;

    public class FileSystemLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        private readonly string path;

        public FileSystemLuceneDirectoryFactory([NotNull] string directory)
        {
            Guard.NotNullOrWhiteSpace(directory, nameof(directory));

            path = directory;
        }

        public Directory Create()
        {
            return FSDirectory.Open(path);
        }
    }
}
