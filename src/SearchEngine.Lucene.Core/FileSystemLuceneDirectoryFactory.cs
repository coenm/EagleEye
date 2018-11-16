namespace SearchEngine.LuceneNet.Core
{
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Lucene.Net.Store;

    public class FileSystemLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        private readonly string path;

        public FileSystemLuceneDirectoryFactory([NotNull] FileSystemLuceneDirectorySettings settings)
        {
            Guard.NotNull(settings, nameof(settings));
            Guard.NotNullOrWhiteSpace(settings.Directory, nameof(settings.Directory));

            path = settings.Directory;
        }

        public Directory Create()
        {
            return FSDirectory.Open(path);
        }
    }
}
