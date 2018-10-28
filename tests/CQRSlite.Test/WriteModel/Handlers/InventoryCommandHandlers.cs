namespace CQRSlite.Test.WriteModel.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Test.WriteModel.Commands;
    using CQRSlite.Test.WriteModel.Domain;

    public class InventoryCommandHandlers : ICancellableCommandHandler<CreateInventoryItem>,
//                                            ICancellableCommandHandler<DeactivateInventoryItem>,
                                            ICancellableCommandHandler<RemoveItemsFromInventory>
//                                            ICancellableCommandHandler<CheckInItemsToInventory>,
//                                            ICancellableCommandHandler<RenameInventoryItem>
    {
        private readonly ISession session;

        public InventoryCommandHandlers(ISession session)
        {
            this.session = session;
        }

        public async Task Handle(CreateInventoryItem message, CancellationToken token = new CancellationToken())
        {
            var item = new InventoryItem(message.Id, message.Name);
            await session.Add(item, token).ConfigureAwait(false);
            await session.Commit(token).ConfigureAwait(false);
        }

//        public async Task Handle(DeactivateInventoryItem message, CancellationToken token)
//        {
//            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
//            item.Deactivate();
//            await _session.Commit(token);
//        }

        public async Task Handle(RemoveItemsFromInventory message, CancellationToken token)
        {
            var item = await session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
            item.Remove(message.Count);
            await session.Commit(token);
        }

//        public async Task Handle(CheckInItemsToInventory message, CancellationToken token)
//        {
//            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
//            item.CheckIn(message.Count);
//            await _session.Commit(token);
//        }
//
//        public async Task Handle(RenameInventoryItem message, CancellationToken token)
//        {
//            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
//            item.ChangeName(message.NewName);
//            await _session.Commit(token);
//        }
    }
}
