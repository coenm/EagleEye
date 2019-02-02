﻿namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneDirectoryFactories
{
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using Helpers.Guards;
    using JetBrains.Annotations;
    using Lucene.Net.Store;

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