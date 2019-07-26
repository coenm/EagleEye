namespace Photo.ReadModel.Similarity.Test.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.Similarity.Internal.EntityFramework.Models;
    using Microsoft.EntityFrameworkCore;

    internal class EventOnSaveDbContextDecoratorFactory : ISimilarityDbContextFactory
    {
        private readonly ISimilarityDbContextFactory decoratee;
        private readonly ISimilarityDbContextSavedEventPublisher eventBus;

        public EventOnSaveDbContextDecoratorFactory(ISimilarityDbContextSavedEventPublisher eventBus, ISimilarityDbContextFactory decoratee)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
        }

        public Task Initialize() => decoratee.Initialize();

        public ISimilarityDbContext CreateDbContext()
        {
            return new EventOnSaveDbContextDecorator(
                                                     decoratee.CreateDbContext(),
                                                     eventBus);
        }

        private class EventOnSaveDbContextDecorator : ISimilarityDbContext
        {
            private readonly ISimilarityDbContext decoratee;
            private readonly ISimilarityDbContextSavedEventPublisher eventBus;

            public EventOnSaveDbContextDecorator(ISimilarityDbContext decoratee, ISimilarityDbContextSavedEventPublisher eventBus)
            {
                this.decoratee = decoratee ?? throw new ArgumentNullException(nameof(decoratee));
                this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            }

            public void Dispose()
            {
                decoratee.Dispose();
            }

            public DbSet<HashIdentifiers> HashIdentifiers => decoratee.HashIdentifiers;

            public DbSet<PhotoHash> PhotoHashes => decoratee.PhotoHashes;

            public DbSet<Scores> Scores => decoratee.Scores;

            public async Task SaveChangesAsync(CancellationToken ct = default)
            {
                await decoratee.SaveChangesAsync(ct);
                eventBus.Publish();
            }

            public void SaveChanges()
            {
                decoratee.SaveChanges();
                eventBus.Publish();
            }
        }
    }
}
