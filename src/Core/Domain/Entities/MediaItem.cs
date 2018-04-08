namespace EagleEye.Core.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CQRSlite.Domain;

    using EagleEye.Core.Domain.Events;

    using JetBrains.Annotations;

    public class MediaItem : AggregateRoot
    {
        private bool _created;

        private readonly List<string> _tags;

        public MediaItem(Guid id, string name) : this()
        {
            Id = id;
            ApplyChange(new MediaItemCreated(id, name));
        }

        private MediaItem()
        {
            _tags = new List<string>();
        }

        public void AddTags(params string[] tags)
        {
            if (tags == null)
                return;

            var addedTags = new List<string>(tags.Length);

            foreach (var tag in tags.Distinct())
            {
                if (!_tags.Contains(tag))
                    addedTags.Add(tag);
            }

            if (addedTags.Any())
                ApplyChange(new TagsAddedToMediaItem(Id, addedTags.ToArray()));
        }

        public void RemoveTags(params string[] tags)
        {
            if (tags == null)
                return;

            var tagsRemoved = new List<string>(tags.Length);

            foreach (var tag in tags.Distinct())
            {
                if (_tags.Contains(tag))
                    tagsRemoved.Add(tag);
            }

            if (tagsRemoved.Any())
                ApplyChange(new TagsRemovedFromMediaItem(Id, tagsRemoved.ToArray()));
        }

        [UsedImplicitly]
        private void Apply(MediaItemCreated e)
        {
            _created = true;
        }

        [UsedImplicitly]
        private void Apply(TagsAddedToMediaItem e)
        {
            _tags.AddRange(e.Tags ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(TagsRemovedFromMediaItem e)
        {
            if (e.Tags == null)
                return;

            foreach (var t in e.Tags)
                _tags.Remove(t);
        }
    }
}