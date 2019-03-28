namespace CQRSlite.Test.WriteModel.Domain
{
    using System;

    using CQRSlite.Domain;
    using CQRSlite.Test.Events;
    using Dawn;
    using JetBrains.Annotations;

    public class InventoryItem : AggregateRoot
    {
        private bool activated;

        public InventoryItem(Guid id, string name)
        {
            Id = id;
            ApplyChange(new InventoryItemCreated(id, name));
        }

        private InventoryItem()
        {
        }

        public void ChangeName([NotNull] string newName)
        {
            Dawn.Guard.Argument(newName, nameof(newName)).NotNull().NotEmpty();
            ApplyChange(new InventoryItemRenamed(Id, newName));
        }

        public void Remove(int count)
        {
            if (count <= 0)
                throw new InvalidOperationException("cant remove negative count from inventory");
            ApplyChange(new ItemsRemovedFromInventory(Id, count));
        }

        public void CheckIn(int count)
        {
            if (count <= 0)
                throw new InvalidOperationException("must have a count greater than 0 to add to inventory");
            ApplyChange(new ItemsCheckedInToInventory(Id, count));
        }

        public void Deactivate()
        {
            if (!activated)
                throw new InvalidOperationException("already deactivated");
            ApplyChange(new InventoryItemDeactivated(Id));
        }

        private void Apply(InventoryItemCreated e)
        {
            activated = true;
        }

        private void Apply(InventoryItemDeactivated e)
        {
            activated = false;
        }
    }
}
