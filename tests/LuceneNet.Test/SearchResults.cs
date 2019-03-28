namespace EagleEye.LuceneNet.Test
{
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class SearchResults<T>
        where T : class, new()
    {
        public SearchResults([NotNull] T data, float score)
        {
            Dawn.Guard.Argument(data, nameof(data)).NotNull();
            Data = data;
            Score = score;
        }

        public T Data { get; }

        public float Score { get; }
    }
}
