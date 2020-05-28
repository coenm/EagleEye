namespace EagleEye.EventStore.NEventStoreAdapter
{
    using System;

    using CQRSlite.Events;
    using Dawn;

    using EagleEye.Core.Interfaces.Core;

    using JetBrains.Annotations;
    using NEventStore;
    using NEventStore.Serialization.Json;

    [UsedImplicitly]
    public class NEventStoreAdapterInMemoryFactory : INEventStoreAdapterFactory, INEventStoreEventExporterAdapterFactory, IDisposable
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

        IEventExporter INEventStoreEventExporterAdapterFactory.Create()
        {
            return new NEventStoreEventExporter(store);
        }
    }
}
