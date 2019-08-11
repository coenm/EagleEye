namespace CQRSlite.Test.EventHandlers
{
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using CQRSlite.Test.Events;

    public class InventoryItemCreatedEventHandler : IEventHandler<InventoryItemCreated>
    {
        public Task Handle(InventoryItemCreated message)
        {
            return Task.FromResult(0);
        }
    }
}
