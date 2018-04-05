namespace EagleEye.Core.Domain.Entities
{
    using System;

    using CQRSlite.Domain;

    using EagleEye.Core.Domain.Events;

    using JetBrains.Annotations;

    public class MediaItem : AggregateRoot
    {
        private bool _created;

        public MediaItem(Guid id, string name)
        {
            Id = id;
            ApplyChange(new MediaItemCreated(id, name));
        }

        [UsedImplicitly]
        private void Apply(MediaItemCreated e)
        {
            _created = true;
        }
    }
}