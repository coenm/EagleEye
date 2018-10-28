namespace SearchEngine.LuceneNet.Core.Queries.WildcardSearchQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using SearchEngine.Interface.Queries;
    using SearchEngine.LuceneNet.Core.Index;
    using SearchEngine.LuceneNet.Core.Queries;

    public class WildcardSearchQueryHandler : IQueryHandler<WildcardSearchQuery, SearchResult>
    {
        private readonly MediaIndex index;

        public WildcardSearchQueryHandler([NotNull] MediaIndex index)
        {
            this.index = index;
        }

        public Task<SearchResult> HandleAsync(WildcardSearchQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var searchResult = index.Search(query.Query, out var hitsCount);

            var result = new SearchResult
            {
                Count = hitsCount,
                Items = searchResult
                    .Select(item =>
                        new SearchItem
                        {
                            Filename = item.FileInformation?.Filename,
                            Score = item.Score,
                            Guid = Guid.NewGuid(),
                        })
                    .ToArray(),
            };

            return Task.FromResult(result);
        }
    }
}
