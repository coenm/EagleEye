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
    using Microsoft.EntityFrameworkCore;

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
            using var db = contextFactory.CreateDbContext();
            var result = await repository.GetAllHashIdentifiersAsync(db).ConfigureAwait(false);
            return result?.Select(x => x.HashIdentifier).ToList() ?? new List<string>(0);
        }

        public async Task<int> CountSimilaritiesAsync(Guid photoGuid, string hashAlgorithm, double scoreThreshold)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (hashAlgorithm == null)
                return 0;

            using var db = contextFactory.CreateDbContext();
            var hashAlgorithms = await repository.GetAllHashIdentifiersAsync(db).ConfigureAwait(false);

            var singleHashAlgorithm = hashAlgorithms?.SingleOrDefault(x => x.HashIdentifier == hashAlgorithm);
            if (singleHashAlgorithm == null)
                return 0;

            return await repository
                         .GetScoresForPhotoAndHashIdentifier(db, photoGuid, singleHashAlgorithm)
                         .CountAsync(s => s.Score >= scoreThreshold)
                         .ConfigureAwait(false);
        }

        public async Task<SimilarityResultSet> GetSimilaritiesAsync(Guid photoGuid, string hashAlgorithm, float scoreThreshold)
        {
            SimilarityResultSet CreateResult(params SimilarityResult[] resultSet)
            {
                return new SimilarityResultSet(
                                               photoGuid,
                                               dateTimeService.Now,
                                               resultSet);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable once HeuristicUnreachableCode
            if (hashAlgorithm == null)
                return CreateResult();

            using var db = contextFactory.CreateDbContext();
            var hashAlgorithms = await repository.GetAllHashIdentifiersAsync(db).ConfigureAwait(false);

            var singleHashAlgorithm = hashAlgorithms.SingleOrDefault(x => x.HashIdentifier == hashAlgorithm);
            if (singleHashAlgorithm == null)
                return CreateResult();

            var matches = await repository
                                .GetScoresForPhotoAndHashIdentifier(db, photoGuid, singleHashAlgorithm)
                                .Where(s => s.Score >= scoreThreshold)
                                .Select(s => CreateSimilarityResult(photoGuid, s.PhotoA, s.PhotoB, s.Score))
                                .ToArrayAsync()
                                .ConfigureAwait(false);

            return CreateResult(matches);
        }

        private static SimilarityResult CreateSimilarityResult(Guid queried, Guid photoA, Guid photoB, double score)
        {
            if (queried != photoA)
                return new SimilarityResult(photoA, score);
            return new SimilarityResult(photoB, score);
        }
    }
}
