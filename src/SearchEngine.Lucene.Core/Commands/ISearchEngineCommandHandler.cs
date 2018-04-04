namespace SearchEngine.LuceneNet.Core.Commands
{
    using System.Threading.Tasks;

    public interface ICommandHandler<TCommand>
    {
        Task HandleAsync(TCommand command);
    }
}