namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;

    using CQRSlite.Events;
    using Dawn;
    using JetBrains.Annotations;
    using NEventStore;
    using NEventStore.Serialization.Json;

    public class NEventStoreAdapterFactory : IDisposable
    {
        [NotNull] private readonly IStoreEvents store;

        public NEventStoreAdapterFactory()
        {
            store = Wireup.Init()
                          .UseOptimisticPipelineHook()
                          .UsingInMemoryPersistence()
                          .InitializeStorageEngine()
                          .UsingJsonSerialization()
                          .Build();
        }

        public NEventStoreAdapter Create([NotNull] IEventPublisher publisher)
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
