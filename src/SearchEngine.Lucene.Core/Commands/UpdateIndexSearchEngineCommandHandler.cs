namespace SearchEngine.LuceneNet.Core.Commands
{
    using System.Threading.Tasks;

    using SearchEngine.Interface.Commands;
    using SearchEngine.LuceneNet.Core.Index;

    public class UpdateIndexSearchEngineCommandHandler : ICommandHandler<UpdateIndexCommand>
    {
        private readonly MediaIndex mediaIndex;

        public UpdateIndexSearchEngineCommandHandler(MediaIndex mediaIndex)
        {
            this.mediaIndex = mediaIndex;
        }

        public Task HandleAsync(UpdateIndexCommand command)
        {
            return mediaIndex.IndexMediaFileAsync(command.Data);
        }
    }
}
