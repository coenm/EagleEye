namespace EagleEye.FileImporter.Indexing
{
    using System;

    public class PersistantFileIndexService
    {
        private readonly IImageDataRepository repository;

        public PersistantFileIndexService(IImageDataRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void AddOrUpdate(ImageData imageData)
        {
            repository.AddOrUpdate(imageData);
        }

        public void Delete(string identifier)
        {
            var fileIndex = repository.Get(identifier);
            if (fileIndex == null)
                return;

            repository.Delete(fileIndex);
        }
    }
}