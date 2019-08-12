namespace CQRSlite.Test.EventHandlers
{
    using System.Threading.Tasks;

    using CQRSlite.Events;
    using CQRSlite.Test.Events;

    public class InventoryItemDeactivatedEventHandler : IEventHandler<InventoryItemDeactivated>
    {
        public Task Handle(InventoryItemDeactivated message)
        {
            return Task.FromResult(0);
        }
    }
}
