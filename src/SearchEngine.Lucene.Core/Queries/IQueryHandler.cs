namespace SearchEngine.LuceneNet.Core.Queries
{
    using System.Threading.Tasks;

    using SearchEngine.Interface;

    public interface IQueryHandler<TQuery, TResult>
        where TQuery : ISearchEngineQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery query);
    }
}
