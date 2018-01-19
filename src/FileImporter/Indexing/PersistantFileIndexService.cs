using System;

namespace FileImporter.Indexing
{
    public class PersistantFileIndexService
    {
        private readonly IFileIndexRepository _repository;

        public PersistantFileIndexService(IFileIndexRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void AddOrUpdate(ImageData imageData)
        {
            _repository.AddOrUpdate(imageData);
        }

        public void Delete(string identifier)
        {
            var fileIndex = _repository.Get(identifier);
            if (fileIndex == null)
                return;

            _repository.Delete(fileIndex);
        }
    }
}