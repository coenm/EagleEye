namespace EagleEye.FileImporter.Indexing
{
    using System;
    using System.Collections.Generic;

    public interface IImageDataRepository
    {
        ImageData Get(string identifier);

        IEnumerable<ImageData> Find(Predicate<ImageData> predicate, int take = 0, int skip = 0);

        IEnumerable<ImageData> FindSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0);

        void Delete(ImageData imageData);

        void AddOrUpdate(ImageData imageData);
    }
}
