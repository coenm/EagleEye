namespace EagleEye.Photo.ReadModel.Similarity.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Photo.ReadModel.Similarity.Interface;
    using EagleEye.Photo.ReadModel.Similarity.Interface.Model;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using JetBrains.Annotations;

    internal class SimilarityReadModel : ISimilarityReadModel
    {
        [NotNull] private readonly IInternalStatelessSimilarityRepository repository;
        [NotNull] private readonly ISimilarityDbContextFactory contextFactory;
        [NotNull] private readonly IDateTimeService dateTimeService;

        public SimilarityReadModel(
            [NotNull] IInternalStatelessSimilarityRepository repository,
            [NotNull] ISimilarityDbContextFactory contextFactory,
            [NotNull] IDateTimeService dateTimeService)
        {
            Guard.Argument(repository, nameof(repository)).NotNull();
            Guard.Argument(contextFactory, nameof(contextFactory)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            this.repository = repository;
            this.contextFactory = contextFactory;
            this.dateTimeService = dateTimeService;
        }

        public async Task<List<string>> GetHashAlgorithmsAsync()
        {
            using (var db = contextFactory.CreateDbContext())
            {
                var result = await repository.GetAllHashIdentifiersAsync(db).ConfigureAwait(false);
                return result.Select(x => x.HashIdentifier).ToList();
            }
        }

        public Task<int> CountSimilaritiesAsync(Guid photoGuid, string hashAlgorithm, double scoreThreshold)
        {
            using (var db = contextFactory.CreateDbContext())
            {
                // todo
                return Task.FromResult(42);
            }
        }

        public Task<SimilarityResultSet> GetSimilaritiesAsync(Guid photoGuid, string hashAlgorithm, float scoreThreshold)
        {
            return Task.FromResult(
               new SimilarityResultSet(photoGuid, dateTimeService.Now, new SimilarityResult[0]));
        }
    }
}
