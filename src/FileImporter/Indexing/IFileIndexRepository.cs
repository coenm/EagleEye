using System;
using System.Collections.Generic;

namespace FileImporter.Indexing
{
    public interface IFileIndexRepository
    {
        FileIndex Get(string identifier);

        IEnumerable<FileIndex> Find(Predicate<FileIndex> predicate, int take = 0, int skip = 0);

        IEnumerable<FileIndex> FindSimilar(FileIndex src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0);

        int Count(Predicate<FileIndex> predicate);

        int CountSimilar(FileIndex src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95);

        void Delete(FileIndex fileIndex);

        void AddOrUpdate(FileIndex fileIndex);
    }
}