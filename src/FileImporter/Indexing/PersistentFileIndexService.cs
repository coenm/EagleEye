namespace EagleEye.FileImporter.Indexing
{
    using Helpers.Guards; using Dawn;

    public class PersistentFileIndexService
    {
        private readonly IImageDataRepository repository;

        public PersistentFileIndexService(IImageDataRepository repository)
        {
            Helpers.Guards.Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
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
