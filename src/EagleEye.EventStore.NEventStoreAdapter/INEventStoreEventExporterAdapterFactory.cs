namespace EagleEye.EventStore.NEventStoreAdapter
{
    using EagleEye.Core.Interfaces.Core;

    public interface INEventStoreEventExporterAdapterFactory
    {
        IEventExporter Create();
    }
}
