namespace SearchEngine.LuceneNet.Core.Commands
{
    public interface ICommandHandler
    {
        bool CanHandle(ISearchEngineCommand command);

        void Execute(ISearchEngineCommand command);
    }
}