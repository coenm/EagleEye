namespace SearchEngine.LuceneNet.Core.Commands
{
    public interface ICommandHandler<TCommand>
    {
        void Handle(TCommand command);
    }
}