namespace Photo.ReadModel.Similarity.Test.Integration
{
    using System;

    public interface ISimilarityDbContextSavedEventPublisher
    {
        event EventHandler DbSaveHappened;

        void Publish();
    }
}
