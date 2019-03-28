namespace EagleEye.Photo.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CQRSlite.Domain;
    using EagleEye.Photo.Domain.Events;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class Photo : AggregateRoot
    {
        private const int Sha256ByteSize = 256 / 8;
        [NotNull] private readonly Dictionary<string, ulong> photoHashes;
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
            Helpers.Guards.Guard.NotEmpty(id, nameof(id));
            Helpers.Guards.Guard.NotNullOrWhiteSpace(filename, nameof(filename));
            Helpers.Guards.Guard.NotNullOrWhiteSpace(mimeType, nameof(mimeType));
            Helpers.Guards.Guard.NotNull(fileSha256, nameof(fileSha256));
            Helpers.Guards.Guard.MustBeEqualTo(fileSha256.Length, Sha256ByteSize, $"{nameof(fileSha256)}.{nameof(fileSha256.Length)}");

            Id = id;
            ApplyChange(new PhotoCreated(id, filename, mimeType, fileSha256));
        }

        /* Also */ [UsedImplicitly] // by CQRS lite framework
        private Photo()
        {
            tags = new List<string>();
            persons = new List<string>();
            photoHashes = new Dictionary<string, ulong>();
        }

        public IReadOnlyList<string> Persons => persons.AsReadOnly();

        public IReadOnlyList<string> Tags => tags.AsReadOnly();

        public void AddTags(params string[] tags)
        {
            if (tags == null)
                return;

            var addedTags = tags.Distinct()
                                .Where(item =>
                                           !string.IsNullOrWhiteSpace(item)
                                           &&
                                           !this.tags.Contains(item))
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
                               .Where(item =>
                                          !string.IsNullOrWhiteSpace(item)
                                          &&
                                          !this.persons.Contains(item))
                               .ToArray();

            if (added.Any())
                ApplyChange(new PersonsAddedToPhoto(Id, added));
        }

        public void RemovePersons(params string[] persons)
        {
            if (persons == null)
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
            Helpers.Guards.Guard.NotNullOrEmpty(fileHash, nameof(fileHash));

            if (!this.fileHash.SequenceEqual(fileHash))
                ApplyChange(new FileHashUpdated(Id, fileHash));
        }

        public void UpdatePhotoHash([NotNull] string hashIdentifier, ulong fileHash)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));

            if (!photoHashes.ContainsKey(hashIdentifier))
                ApplyChange(new PhotoHashAdded(Id, hashIdentifier, fileHash));
            else if (photoHashes[hashIdentifier] != fileHash)
                ApplyChange(new PhotoHashUpdated(Id, hashIdentifier, fileHash));
        }

        public void ClearPhotoHash([NotNull] string hashIdentifier)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(hashIdentifier, nameof(hashIdentifier));

            if (photoHashes.ContainsKey(hashIdentifier))
                ApplyChange(new PhotoHashCleared(Id, hashIdentifier));
        }

        [UsedImplicitly]
        private void Apply([NotNull] FileHashUpdated e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            fileHash = e.Hash.ToArray();
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashAdded e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            photoHashes.Add(e.HashIdentifier, e.Hash);
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashUpdated e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            photoHashes[e.HashIdentifier] = e.Hash;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashCleared e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            photoHashes.Remove(e.HashIdentifier);
        }

        [UsedImplicitly]
        private void Apply([NotNull] LocationClearedFromPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            location = null;
        }

        [UsedImplicitly]
        private void Apply([NotNull] LocationSetToPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            location = e.Location;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoCreated e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            created = true;
            Id = e.Id;
            filename = e.FileName;
            fileHash = e.FileHash;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PersonsAddedToPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            persons.AddRange(e.Persons);
        }

        [UsedImplicitly]
        private void Apply([NotNull] PersonsRemovedFromPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            foreach (var t in e.Persons)
                persons.Remove(t);
        }

        [UsedImplicitly]
        private void Apply([NotNull] TagsAddedToPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            tags.AddRange(e.Tags ?? new string[0]);
        }

        [UsedImplicitly]
        private void Apply([NotNull] TagsRemovedFromPhoto e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            foreach (var t in e.Tags)
                tags.Remove(t);
        }

        [UsedImplicitly]
        private void Apply([NotNull] DateTimeTakenChanged e)
        {
            DebugHelpers.Guards.Guard.NotNull(e, nameof(e));

            dateTimeTaken = e.DateTimeTaken;
            dateTimeTakenPrecision = e.Precision;
        }
    }
}
