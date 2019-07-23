namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;

    using CQRSlite.Events;
    using Dawn;
    using JetBrains.Annotations;
    using NEventStore;
    using NEventStore.Serialization.Json;

    [UsedImplicitly]
    public class NEventStoreAdapterInMemoryFactory : INEventStoreAdapterFactory, IDisposable
    {
        [NotNull] private readonly IStoreEvents store;

        public NEventStoreAdapterInMemoryFactory()
        {
            store = Wireup.Init()
                          .UseOptimisticPipelineHook()
                          .UsingInMemoryPersistence()
                          .InitializeStorageEngine()
                          .UsingJsonSerialization()
                          .Build();
        }

        public IEventStore Create([NotNull] IEventPublisher publisher)
        {
            Guard.Argument(publisher, nameof(publisher)).NotNull();

            return new NEventStoreAdapter(publisher, store);
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
