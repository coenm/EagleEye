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
        private bool created;

        private readonly List<string> tags;
        private readonly List<string> persons;
        private Location location;
        private string filename;

        public MediaItem(Guid id, string filename, string[] tags, string[] persons)
            : this()
        {
            Id = id;
            location = null;

            ApplyChange(new MediaItemCreated(id, filename, tags, persons));
        }

        private MediaItem()
        {
            tags = new List<string>();
            persons = new List<string>();
        }

        public void AddTags(params string[] tags)
        {
            if (tags == null)
                return;

            var addedTags = tags.Distinct().Where(tag => !this.tags.Contains(tag)).ToArray();

            if (addedTags.Any())
                ApplyChange(new TagsAddedToMediaItem(Id, addedTags));
        }

        public void RemoveTags(params string[] tags)
        {
            if (tags == null)
                return;

            var tagsRemoved = tags.Distinct()
                                  .Where(tag => this.tags.Contains(tag))
                                  .ToArray();

            if (tagsRemoved.Any())
                ApplyChange(new TagsRemovedFromMediaItem(Id, tagsRemoved));
        }

        public void AddPersons(params string[] persons)
        {
            if (persons == null)
                return;

            var added = persons.Distinct()
                               .Where(item => !this.persons.Contains(item))
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
                                 .Where(item => this.persons.Contains(item))
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
            if (location == null)
                return;

            ApplyChange(new LocationClearedFromMediaItem(Id/*, _location*/));
        }

        [UsedImplicitly]
        private void Apply(LocationClearedFromMediaItem e)
        {
            location = null;
        }

        [UsedImplicitly]
        private void Apply(LocationSetToMediaItem e)
        {
            location = e.Location;
        }

        [UsedImplicitly]
        private void Apply(MediaItemCreated e)
        {
            created = true;
            filename = e.FileName;
            tags.AddRange(e.Tags);
            persons.AddRange(e.Persons);
        }

        [UsedImplicitly]
        private void Apply(PersonsAddedToMediaItem e)
        {
            persons.AddRange(e.Persons ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(PersonsRemovedFromMediaItem e)
        {
            if (e.Persons == null)
                return;

            foreach (var t in e.Persons)
                persons.Remove(t);
        }

        [UsedImplicitly]
        private void Apply(TagsAddedToMediaItem e)
        {
            tags.AddRange(e.Tags ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(TagsRemovedFromMediaItem e)
        {
            if (e.Tags == null)
                return;

            foreach (var t in e.Tags)
                tags.Remove(t);
        }
    }
}
