namespace EagleEye.EventStore.NEventStoreAdapter
{
    using CQRSlite.Events;

    using JetBrains.Annotations;

    public interface INEventStoreAdapterFactory
    {
        IEventStore Create([NotNull] IEventPublisher publisher);
    }
}