namespace EagleEye.FileImporter.Infrastructure.JsonSimilarity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EagleEye.FileImporter.Similarity;

    public class SingleFileSimilarityRepository : ISimilarityRepository
    {
        private readonly IPersistentSerializer<List<SimilarityResultStorage>> storage;
        private readonly List<SimilarityResultStorage> data;
        private readonly object syncLock = new object();
        private bool autoSave = true;

        public SingleFileSimilarityRepository(IPersistentSerializer<List<SimilarityResultStorage>> storage)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(this.storage));
            data = this.storage.Load();
        }

        public IEnumerable<byte[]> FindAllRecordedMatches(byte[] contentHash)
        {
            if (contentHash == null)
                throw new ArgumentNullException(nameof(contentHash));

            return data
                   .Where(index => index.ImageHash.Contains(contentHash))
                   .Select(index => index.ImageHash.Single(y => y.SequenceEqual(contentHash) == false));
        }

        public IEnumerable<SimilarityResult> FindSimilar(byte[] contentHash, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0)
        {
            if (contentHash == null)
                throw new ArgumentNullException(nameof(contentHash));

            // ReSharper disable once InconsistentlySynchronizedField
            IEnumerable<SimilarityResultStorage> result = data.Where(index =>
                    index.ImageHash.Contains(contentHash)
                    &&
                    index.AverageHash >= minAvgHash
                    &&
                    index.DifferenceHash >= minDiffHash
                    &&
                    index.PerceptualHash >= minPerHash)
                .OrderBy(x => x.DifferenceHash)
                .ThenBy(x => x.PerceptualHash)
                .ThenBy(x => x.AverageHash);

            if (skip > 0)
                result = result.Skip(skip);

            if (take > 0)
                result = result.Take(take);

            return result.Select(match => new SimilarityResult
            {
                DifferenceHash = match.DifferenceHash,
                AverageHash = match.AverageHash,
                PerceptualHash = match.PerceptualHash,
                OtherImageHash = match.ImageHash.Single(y => y.SequenceEqual(contentHash) == false),
            });
        }

        public int CountSimilar(byte[] contentHash, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95)
        {
            return FindSimilar(contentHash, minAvgHash, minDiffHash, minPerHash).Count();
        }

        public void Delete(byte[] contentHash)
        {
            if (contentHash == null)
                throw new ArgumentNullException(nameof(contentHash));

            lock (syncLock)
            {
                var existingItems = data.Where(index => index.ImageHash.Contains(contentHash)).ToArray();

                if (existingItems.Any())
                    return;

                foreach (var item in existingItems)
                    data.Remove(item);

                storage.Save(data);
            }
        }

        public void AddOrUpdate(byte[] contentHash, SimilarityResult similarity)
        {
            if (contentHash == null)
                throw new ArgumentNullException(nameof(contentHash));

            if (similarity == null)
                throw new ArgumentNullException(nameof(similarity));

            lock (syncLock)
            {
                var existingItem = data.FirstOrDefault(index => index.ImageHash.Contains(contentHash)
                                                                &&
                                                                index.ImageHash.Contains(similarity.OtherImageHash));

                if (existingItem != null)
                    data.Remove(existingItem);

                data.Add(new SimilarityResultStorage
                              {
                                  AverageHash = similarity.AverageHash,
                                  DifferenceHash = similarity.DifferenceHash,
                                  PerceptualHash = similarity.PerceptualHash,
                                  ImageHash = new List<byte[]>(2)
                                                  {
                                                      contentHash,
                                                      similarity.OtherImageHash,
                                                  },
                              });

                if (autoSave)
                    storage.Save(data);
            }
        }

        public void SaveChanges()
        {
            lock (syncLock)
            {
                storage.Save(data);
            }
        }

        public void AutoSave(bool value)
        {
            lock (syncLock)
            {
                autoSave = value;
            }
        }
    }
}
