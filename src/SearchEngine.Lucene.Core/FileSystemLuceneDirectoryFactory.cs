namespace SearchEngine.LuceneNet.Core
{
    using System;

    using JetBrains.Annotations;

    using Lucene.Net.Store;

    public class FileSystemLuceneDirectoryFactory : ILuceneDirectoryFactory
    {
        private readonly string _path;

        public FileSystemLuceneDirectoryFactory([NotNull] FileSystemLuceneDirectorySettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Directory))
                throw new ArgumentNullException(nameof(settings));

            _path = settings.Directory;
        }

        public Directory Create()
        {
            return FSDirectory.Open(_path);
        }
    }
}