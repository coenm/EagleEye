using System;
using System.Collections.Generic;
using System.Linq;
using FileImporter.Indexing;

namespace FileImporter.Infrastructure.FileIndexRepository
{
    /// <summary>
    /// Stores data in file.
    /// </summary>
    public class SingleFileIndexRepository : IFileIndexRepository
    {
        private readonly IPersistantSerializer<List<FileIndex>> _storage;
        private readonly List<FileIndex> _data;
        private readonly object _syncLock = new object();

        public SingleFileIndexRepository(IPersistantSerializer<List<FileIndex>> storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(_storage));
            _data = _storage.Load();
        }

        public FileIndex Get(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException(nameof(identifier));

            return _data.FirstOrDefault(i => i.Identifier.Equals(identifier, StringComparison.InvariantCulture));
        }

        public IEnumerable<FileIndex> Find(Predicate<FileIndex> predicate, int take = 0, int skip = 0)
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

        public IEnumerable<FileIndex> FindSimilar(FileIndex src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95,
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
                });

            if (skip > 0)
                result = result.Skip(skip);

            if (take > 0)
                result = result.Take(take);

            return result;
        }

        public int Count(Predicate<FileIndex> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _data.Count(index => predicate(index));
        }

        public int CountSimilar(FileIndex src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95)
        {
            return FindSimilar(src, minAvgHash, minDiffHash, minPerHash).Count();
        }


        public void Delete(FileIndex item)
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

        public void AddOrUpdate(FileIndex item)
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