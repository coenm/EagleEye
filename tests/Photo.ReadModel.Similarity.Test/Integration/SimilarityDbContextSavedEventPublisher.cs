namespace Photo.ReadModel.Similarity.Test.Integration
{
    using System;

    internal class SimilarityDbContextSavedEventPublisher : ISimilarityDbContextSavedEventPublisher
    {
        public event EventHandler DbSaveHappened;

        public void Publish()
        {
            DbSaveHappened?.Invoke(this, new EventArgs());
        }
    }
}
