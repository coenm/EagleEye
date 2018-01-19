using System;
using System.Collections.Generic;

namespace FileImporter.Indexing
{
    public interface IFileIndexRepository
    {
        ImageData Get(string identifier);

        IEnumerable<ImageData> Find(Predicate<ImageData> predicate, int take = 0, int skip = 0);

        IEnumerable<ImageData> FindSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0);

        IEnumerable<ImageData> FindByContentHash(byte[] imageHash);

        IEnumerable<ImageData> FindImageHashesNotInList(IEnumerable<byte[]> imageHashes);

        int Count(Predicate<ImageData> predicate);

        int CountSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95);

        void Delete(ImageData imageData);

        void AddOrUpdate(ImageData imageData);
    }
}