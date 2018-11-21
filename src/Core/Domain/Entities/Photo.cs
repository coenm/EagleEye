namespace EagleEye.Core.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CQRSlite.Domain;

    using EagleEye.Core.Domain.Events;
    using Helpers.Guards;
    using JetBrains.Annotations;

    public class Photo : AggregateRoot
    {
        private const int Sha256ByteSize = 256 / 8;
        [NotNull] private readonly List<string> tags;
        [NotNull] private readonly List<string> persons;

        private bool created;
        [CanBeNull] private Location location;
        private string filename;
        private byte[] fileHash;

        public Photo(
            Guid id,
            [NotNull] string filename,
            [NotNull] byte[] fileSha256,
            [CanBeNull] string[] tags,
            [CanBeNull] string[] persons)
            : this()
        {
            Guard.NotNullOrWhiteSpace(filename, nameof(filename));
            Guard.NotNull(fileSha256, nameof(fileSha256));
            Guard.MustBeEqualTo(fileSha256.Length, Sha256ByteSize, $"{nameof(fileSha256)}.{nameof(fileSha256.Length)}");

            Id = id;

            ApplyChange(new PhotoCreated(id, filename, fileSha256, tags, persons));
        }

        private Photo()
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
                ApplyChange(new TagsAddedToPhoto(Id, addedTags));
        }

        public void RemoveTags(params string[] tags)
        {
            if (tags == null)
                return;

            var tagsRemoved = tags.Distinct()
                                  .Where(tag => this.tags.Contains(tag))
                                  .ToArray();

            if (tagsRemoved.Any())
                ApplyChange(new TagsRemovedFromPhoto(Id, tagsRemoved));
        }

        public void AddPersons(params string[] persons)
        {
            if (persons == null)
                return;

            var added = persons.Distinct()
                               .Where(item => !this.persons.Contains(item))
                               .ToArray();

            if (added.Any())
                ApplyChange(new PersonsAddedToPhoto(Id, added));
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
                ApplyChange(new PersonsRemovedFromPhoto(Id, removed));
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

            ApplyChange(new LocationSetToPhoto(Id, location));
        }

        public void ClearLocationData()
        {
            if (location == null)
                return;

            ApplyChange(new LocationClearedFromPhoto(Id/*, _location*/));
        }

        [UsedImplicitly]
        private void Apply(LocationClearedFromPhoto e)
        {
            location = null;
        }

        [UsedImplicitly]
        private void Apply(LocationSetToPhoto e)
        {
            location = e.Location;
        }

        [UsedImplicitly]
        private void Apply(PhotoCreated e)
        {
            created = true;
            Id = e.Id;
            filename = e.FileName;
            fileHash = e.FileHash;
            tags.AddRange(e.Tags);
            persons.AddRange(e.Persons);
        }

        [UsedImplicitly]
        private void Apply(PersonsAddedToPhoto e)
        {
            persons.AddRange(e.Persons ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(PersonsRemovedFromPhoto e)
        {
            if (e.Persons == null)
                return;

            foreach (var t in e.Persons)
                persons.Remove(t);
        }

        [UsedImplicitly]
        private void Apply(TagsAddedToPhoto e)
        {
            tags.AddRange(e.Tags ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply(TagsRemovedFromPhoto e)
        {
            if (e.Tags == null)
                return;

            foreach (var t in e.Tags)
                tags.Remove(t);
        }
    }
}
