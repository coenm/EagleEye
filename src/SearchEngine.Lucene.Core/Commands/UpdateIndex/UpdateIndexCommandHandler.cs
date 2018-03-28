namespace SearchEngine.LuceneNet.Core.Commands.UpdateIndex
{
    public class UpdateIndexCommandHandler : ICommandHandler
    {
        public bool CanHandle(ISearchEngineCommand command)
        {
            return command is UpdateIndexCommand;
        }

        public void Execute(ISearchEngineCommand command)
        {
            var updateIndexCommand = command as UpdateIndexCommand;


            return;
        }
    }
}