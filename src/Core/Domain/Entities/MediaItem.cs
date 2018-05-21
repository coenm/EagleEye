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
        private readonly List<string> _persons;
        private Location _location;

        public MediaItem(Guid id, string name, string[] tags, string[] persons)
            : this()
        {
            Id = id;
            _location = null;

            ApplyChange(new MediaItemCreated(id, name, tags, persons));
        }

        private MediaItem()
        {
            _tags = new List<string>();
            _persons = new List<string>();
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

        public void AddPersons(params string[] persons)
        {
            if (persons == null)
                return;

            var added = new List<string>(persons.Length);

            foreach (var item in persons.Distinct())
            {
                if (!_persons.Contains(item))
                    added.Add(item);
            }

            if (added.Any())
                ApplyChange(new PersonsAddedToMediaItem(Id, added.ToArray()));
        }

        public void RemovePersons(params string[] persons)
        {
            if (persons == null)
                return;

            if (persons.Length == 0)
                return;

            var removed = new List<string>(persons.Length);

            foreach (var item in persons.Distinct())
            {
                if (_persons.Contains(item))
                    removed.Add(item);
            }

            if (removed.Any())
                ApplyChange(new PersonsRemovedFromMediaItem(Id, removed.ToArray()));
        }

        public void SetLocation(
            string countryCode,
            string countryName,
            string state,
            string city,
            string subLocation,
            float? longitude,
            float? latitude)
        {

            var location = new Location(countryCode, countryName, state, city, subLocation, longitude, latitude);


            ApplyChange(new LocationSetToMediaItem(Id, location));
        }

        public void ClearLocationData()
        {
            if (_location == null)
                return;

            ApplyChange(new LocationClearedFromMediaItem(Id/*, _location*/));
        }

        [UsedImplicitly]
        private void Apply(LocationClearedFromMediaItem e)
        {
            _location = null;
        }

        [UsedImplicitly]
        private void Apply(LocationSetToMediaItem e)
        {
            _location = e.Location;
        }

        [UsedImplicitly]
        private void Apply(MediaItemCreated e)
        {
            _created = true;
            _tags.AddRange(e.Tags);
            _persons.AddRange(e.Persons);
        }

        [UsedImplicitly]
        private void Apply(PersonsAddedToMediaItem e)
        {
            _persons.AddRange(e.Persons ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(PersonsRemovedFromMediaItem e)
        {
            if (e.Persons == null)
                return;

            foreach (var t in e.Persons)
                _persons.Remove(t);
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