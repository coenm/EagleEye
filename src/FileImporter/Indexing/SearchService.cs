namespace EagleEye.FileImporter.Indexing
{
    using System.Collections.Generic;

    using Helpers.Guards;

    public class SearchService
    {
        private readonly IImageDataRepository repository;

        public SearchService(IImageDataRepository repository)
        {
            Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
        }

        public ImageData FindById(string identifier)
        {
            return repository.Get(identifier);
        }

        public IEnumerable<ImageData> FindSimilar(ImageData src, double minAvgHash = 95, double minDiffHash = 95, double minPerHash = 95, int take = 0, int skip = 0)
        {
            return repository.FindSimilar(src, minAvgHash, minDiffHash, minPerHash, take, skip);
        }

        public IEnumerable<ImageData> FindAll()
        {
            return repository.Find(p => true);
        }
    }
}
