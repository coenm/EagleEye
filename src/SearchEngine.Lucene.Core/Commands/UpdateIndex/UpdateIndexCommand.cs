namespace SearchEngine.LuceneNet.Core.Commands.UpdateIndex
{
    public class UpdateIndexCommand : ISearchEngineCommand
    {
        public MediaObject Data { get; set; }
    }
}