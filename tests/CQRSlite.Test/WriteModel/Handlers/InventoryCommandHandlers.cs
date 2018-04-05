﻿namespace CQRSlite.Test.WriteModel.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using CQRSlite.Domain;
    using CQRSlite.Test.WriteModel.Commands;
    using CQRSlite.Test.WriteModel.Domain;

    public class InventoryCommandHandlers : ICommandHandler<CreateInventoryItem> ,
//                                            ICancellableCommandHandler<DeactivateInventoryItem>,
                                            ICancellableCommandHandler<RemoveItemsFromInventory>
//                                            ICancellableCommandHandler<CheckInItemsToInventory>,
//                                            ICancellableCommandHandler<RenameInventoryItem>
    {
        private readonly ISession _session;

        public InventoryCommandHandlers(ISession session)
        {
            _session = session;
        }

        public async Task Handle(CreateInventoryItem message)
        {
            var item = new InventoryItem(message.Id, message.Name);
            await _session.Add(item).ConfigureAwait(false);
            await _session.Commit().ConfigureAwait(false);
        }

//        public async Task Handle(DeactivateInventoryItem message, CancellationToken token)
//        {
//            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
//            item.Deactivate();
//            await _session.Commit(token);
//        }
//
        public async Task Handle(RemoveItemsFromInventory message, CancellationToken token)
        {
            var item = await _session.Get<InventoryItem>(message.Id, message.ExpectedVersion, token);
            item.Remove(message.Count);
            await _session.Commit(token);
        }
//
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