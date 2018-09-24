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
        private string _filename;

        public MediaItem(Guid id, string filename, string[] tags, string[] persons)
            : this()
        {
            Id = id;
            _location = null;

            ApplyChange(new MediaItemCreated(id, filename, tags, persons));
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

            var addedTags = tags.Distinct().Where(tag => !_tags.Contains(tag)).ToArray();

            if (addedTags.Any())
                ApplyChange(new TagsAddedToMediaItem(Id, addedTags));
        }

        public void RemoveTags(params string[] tags)
        {
            if (tags == null)
                return;

            var tagsRemoved = tags.Distinct()
                                  .Where(tag => _tags.Contains(tag))
                                  .ToArray();

            if (tagsRemoved.Any())
                ApplyChange(new TagsRemovedFromMediaItem(Id, tagsRemoved));
        }

        public void AddPersons(params string[] persons)
        {
            if (persons == null)
                return;

            var added = persons.Distinct()
                               .Where(item => !_persons.Contains(item))
                               .ToArray();

            if (added.Any())
                ApplyChange(new PersonsAddedToMediaItem(Id, added));
        }

        public void RemovePersons(params string[] persons)
        {
            if (persons == null)
                return;

            if (persons.Length == 0)
                return;


            var removed = persons.Distinct()
                                 .Where(item => _persons.Contains(item))
                                 .ToArray();

            if (removed.Any())
                ApplyChange(new PersonsRemovedFromMediaItem(Id, removed));
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
            _filename = e.FileName;
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