namespace EagleEye.Photo.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Events;
    using Helpers.Guards;
    using JetBrains.Annotations;

    internal class Photo : AggregateRoot
    {
        private const int Sha256ByteSize = 256 / 8;
        [NotNull] private readonly Dictionary<string, byte[]> photoHashes;
        [NotNull] private readonly List<string> tags;
        [NotNull] private readonly List<string> persons;
        private DateTime? dateTimeTaken;

        private bool created;
        [CanBeNull] private Location location;
        private string filename;
        private byte[] fileHash;
        private TimestampPrecision dateTimeTakenPrecision;

        internal Photo(
            Guid id,
            [NotNull] string filename,
            [NotNull] string mimeType,
            [NotNull] byte[] fileSha256)
        : this()
        {
            Guard.NotEmpty(id, nameof(id));
            Guard.NotNullOrWhiteSpace(filename, nameof(filename));
            Guard.NotNullOrWhiteSpace(mimeType, nameof(mimeType));
            Guard.NotNull(fileSha256, nameof(fileSha256));
            Guard.MustBeEqualTo(fileSha256.Length, Sha256ByteSize, $"{nameof(fileSha256)}.{nameof(fileSha256.Length)}");

            Id = id;
            ApplyChange(new PhotoCreated(id, filename, mimeType, fileSha256));
        }

        /* Also */ [UsedImplicitly] // by CQRS lite framework
        private Photo()
        {
            tags = new List<string>();
            persons = new List<string>();
            photoHashes = new Dictionary<string, byte[]>();
        }

        public IReadOnlyList<string> Persons => persons.AsReadOnly();

        public void AddTags(params string[] tags)
        {
            if (tags == null)
                return;

            var addedTags = tags.Distinct()
                                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                                .Where(tag => !this.tags.Contains(tag))
                                .ToArray();

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
                               .Where(item => !string.IsNullOrWhiteSpace(item))
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
            var newLocation = new Location(countryCode, countryName, state, city, subLocation, longitude, latitude);

            ApplyChange(new LocationSetToPhoto(Id, newLocation));
        }

        public void ClearLocationData()
        {
            if (location == null)
                return;

            ApplyChange(new LocationClearedFromPhoto(Id/*, _location*/));
        }

        public void SetDateTimeTaken(DateTime dateTime, TimestampPrecision precision)
        {
            // todo check
            ApplyChange(new DateTimeTakenChanged(Id, dateTime, precision));
        }

        public void UpdateFileHash([NotNull] byte[] fileHash)
        {
            Guard.NotNullOrEmpty(fileHash, nameof(fileHash));

            if (!this.fileHash.SequenceEqual(fileHash))
                ApplyChange(new FileHashUpdated(Id, fileHash));
        }

        public void UpdatePhotoHash([NotNull] string hashIdentifier, [NotNull] byte[] fileHash)
        {
            Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));
            Guard.NotNullOrEmpty(fileHash, nameof(fileHash));

            if (!photoHashes.ContainsKey(hashIdentifier) || !photoHashes[hashIdentifier].SequenceEqual(fileHash))
                ApplyChange(new PhotoHashUpdated(Id, hashIdentifier, fileHash));
        }

        public void ClearPhotoHash([NotNull] string hashIdentifier)
        {
            Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));

            if (photoHashes.ContainsKey(hashIdentifier))
                ApplyChange(new PhotoHashCleared(Id, hashIdentifier));
        }

        [UsedImplicitly]
        private void Apply(FileHashUpdated e)
        {
            fileHash = e.Hash.ToArray();
        }

        [UsedImplicitly]
        private void Apply(PhotoHashUpdated e)
        {
            if (photoHashes.ContainsKey(e.HashIdentifier))
                photoHashes[e.HashIdentifier] = e.Hash.ToArray();
            else
                photoHashes.Add(e.HashIdentifier, e.Hash.ToArray());
        }

        [UsedImplicitly]
        private void Apply(PhotoHashCleared e)
        {
            photoHashes.Remove(e.HashIdentifier);
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
        }

        [UsedImplicitly]
        private void Apply(PersonsAddedToPhoto e)
        {
            persons.AddRange(e.Persons);
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

        [UsedImplicitly]
        private void Apply(DateTimeTakenChanged e)
        {
            dateTimeTaken = e.DateTimeTaken;
            dateTimeTakenPrecision = e.Precision;
        }
    }
}
