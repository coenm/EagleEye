using System;
using System.Collections.Generic;
using System.Linq;

namespace FileImporter.Indexing
{
    public class SearchService
    {
        private readonly IFileIndexRepository _repository;

        public SearchService(IFileIndexRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public IEnumerable<FileIndex> FindSimilar(FileIndex src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0)
        {
            bool Predicate(FileIndex index)
            {
                if (index.Identifier.Equals(src.Identifier, StringComparison.InvariantCulture))
                    return false;

                if (index.Hashes.FileHash.SequenceEqual(src.Hashes.FileHash))
                    return true;

                if (index.Hashes.ImageHash.SequenceEqual(src.Hashes.ImageHash))
                    return true;

                if (minAvgHash >= 0 && minAvgHash <= 100)
                {
                    if (CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.AverageHash, src.Hashes.AverageHash) >= minAvgHash)
                        return true;
                }

                if (minDiffHash >= 0 && minDiffHash <= 100)
                {
                    if (CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.DifferenceHash, src.Hashes.DifferenceHash) >= minDiffHash)
                        return true;
                }

                if (minPerHash >= 0 && minPerHash <= 100)
                {
                    if (CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.PerceptualHash, src.Hashes.PerceptualHash) >= minPerHash)
                        return true;
                }

                return false;
            }

            return _repository.Find(Predicate, take, skip);
        }
    }
}