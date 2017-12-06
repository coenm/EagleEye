using System;
using System.Collections.Generic;

namespace FileImporter.Indexing
{
    public interface IFileIndexRepository
    {
        FileIndex Get(string identifier);

        IEnumerable<FileIndex> Find(Predicate<FileIndex> predicate, int take, int skip);

        int Count(Predicate<FileIndex> predicate);

        void Save(FileIndex item);
    }
}