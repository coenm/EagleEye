using System;
using System.Collections.Generic;
using System.Linq;
using FileImporter.Indexing;
using FileImporter.Json;

namespace FileImporter.Infrastructure.FileIndexRepository
{
    /// <summary>
    /// Stores data in file.
    /// </summary>
    public class SingleFileIndexRepository : IFileIndexRepository
    {
        private readonly string _filename;
        private readonly List<FileIndex> _data;
        private readonly object _syncLock = new object();

        public SingleFileIndexRepository(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
            _data = JsonEncoding.ReadFromFile<List<FileIndex>>(_filename);
        }

        public FileIndex Get(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException(nameof(identifier));

            return _data.FirstOrDefault(i => i.Identifier.Equals(identifier, StringComparison.InvariantCulture));
        }

        public IEnumerable<FileIndex> Find(Predicate<FileIndex> predicate, int take, int skip)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _data.Where(index => predicate(index)).Take(take).Skip(skip);
        }

        public int Count(Predicate<FileIndex> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _data.Count(index => predicate(index));
        }

        public void Save(FileIndex item)
        {
            lock (_syncLock)
            {
                var existingItem = _data.FirstOrDefault(index => index.Identifier.Equals(item.Identifier, StringComparison.InvariantCulture));

                if (existingItem != null)
                    _data.Remove(existingItem);

                _data.Add(item);

                JsonEncoding.WriteDataToJsonFile(_data, _filename);
            }
        }
    }
}