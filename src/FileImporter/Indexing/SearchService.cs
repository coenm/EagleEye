using System;
using System.Collections.Generic;

namespace FileImporter.Indexing
{
    public class SearchService
    {
        private readonly IFileIndexRepository _repository;

        public SearchService(IFileIndexRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public ImageData FindById(string identifier)
        {
            return _repository.Get(identifier);
        }

        public IEnumerable<ImageData> FindSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0)
        {
            return _repository.FindSimilar(src, minAvgHash, minDiffHash, minPerHash, take, skip);
        }

        public IEnumerable<ImageData> FindAll()
        {
            return _repository.Find(p => true);
        }
    }
}