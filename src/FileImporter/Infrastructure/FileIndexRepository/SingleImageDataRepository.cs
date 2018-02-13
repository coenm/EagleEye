using System;
using System.Collections.Generic;
using System.Linq;
using EagleEye.FileImporter.Indexing;

namespace EagleEye.FileImporter.Infrastructure.FileIndexRepository
{
    /// <summary>
    /// Stores data in file.
    /// </summary>
    public class SingleImageDataRepository : IImageDataRepository
    {
        private readonly IPersistantSerializer<List<ImageData>> _storage;
        private readonly List<ImageData> _data;
        private readonly object _syncLock = new object();

        public SingleImageDataRepository(IPersistantSerializer<List<ImageData>> storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(_storage));
            _data = _storage.Load();
        }

        public ImageData Get(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException(nameof(identifier));

            return _data.FirstOrDefault(i => i.Identifier.Equals(identifier, StringComparison.InvariantCulture));
        }

        public IEnumerable<ImageData> Find(Predicate<ImageData> predicate, int take = 0, int skip = 0)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = _data.Where(index => predicate(index));

            if (skip > 0)
                result = result.Skip(skip);

            if (take > 0)
                result = result.Take(take);

            return result;
        }

        public IEnumerable<ImageData> FindSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95,
            int take = 0, int skip = 0)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));


            var result = _data.Where(index =>
                {
                    if (index.Identifier.Equals(src.Identifier, StringComparison.InvariantCulture))
                        return false;

                    if (index.Hashes.FileHash.SequenceEqual(src.Hashes.FileHash))
                        return true;

                    if (index.Hashes.ImageHash.SequenceEqual(src.Hashes.ImageHash))
                        return true;

                    double value;

                    if (minAvgHash >= 0 && minAvgHash <= 100)
                    {
                        value = CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.AverageHash, src.Hashes.AverageHash);
                        if (value >= minAvgHash)
                            return true;
                    }

                    if (minDiffHash >= 0 && minDiffHash <= 100)
                    {
                        value = CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.DifferenceHash, src.Hashes.DifferenceHash);
                        if (value >= minDiffHash)
                            return true;
                    }

                    if (minPerHash >= 0 && minPerHash <= 100)
                    {
                        value = CoenM.ImageSharp.CompareHash.Similarity(index.Hashes.PerceptualHash, src.Hashes.PerceptualHash);
                        if (value >= minPerHash)
                            return true;
                    }

                    return false;
                });

            if (skip > 0)
                result = result.Skip(skip);

            if (take > 0)
                result = result.Take(take);

            return result;
        }

        public IEnumerable<ImageData> FindByContentHash(byte[] imageHash)
        {
            if (imageHash == null)
                throw new ArgumentNullException(nameof(imageHash));

            return Find(index => index.Hashes.ImageHash.SequenceEqual(imageHash));
        }

        public IEnumerable<ImageData> FindImageHashesNotInList(IEnumerable<byte[]> imageHashes)
        {
            if (imageHashes == null)
                throw new ArgumentNullException(nameof(imageHashes));

            var hashes = imageHashes.ToArray();

            if (!hashes.Any())
                return Enumerable.Empty<ImageData>();

            return Find(index => !hashes.Contains(index.Hashes.ImageHash));
        }

        public int Count(Predicate<ImageData> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _data.Count(index => predicate(index));
        }

        public int CountSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95)
        {
            return FindSimilar(src, minAvgHash, minDiffHash, minPerHash).Count();
        }

        public void Delete(ImageData item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_syncLock)
            {
                var existingItem = _data.FirstOrDefault(index => index.Identifier.Equals(item.Identifier, StringComparison.InvariantCulture));

                if (existingItem == null)
                    return;

                _data.Remove(existingItem);
                _storage.Save(_data);
            }
        }

        public void AddOrUpdate(ImageData item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_syncLock)
            {
                var existingItem = _data.FirstOrDefault(index => index.Identifier.Equals(item.Identifier, StringComparison.InvariantCulture));

                if (existingItem != null)
                    _data.Remove(existingItem);

                _data.Add(item);
                _storage.Save(_data);
            }
        }
    }
}