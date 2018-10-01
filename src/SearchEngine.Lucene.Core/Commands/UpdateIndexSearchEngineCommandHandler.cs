namespace SearchEngine.LuceneNet.Core.Commands
{
    using System.Threading.Tasks;

    using SearchEngine.Interface.Commands;
    using SearchEngine.LuceneNet.Core.Index;

    public class UpdateIndexSearchEngineCommandHandler : ICommandHandler<UpdateIndexCommand>
    {
        private readonly MediaIndex _mediaIndex;

        public UpdateIndexSearchEngineCommandHandler(MediaIndex mediaIndex)
        {
            _mediaIndex = mediaIndex;
        }

        public Task HandleAsync(UpdateIndexCommand command)
        {
            return _mediaIndex.IndexMediaFileAsync(command.Data);
        }
    }
}