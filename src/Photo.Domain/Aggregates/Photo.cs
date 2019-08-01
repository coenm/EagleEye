namespace EagleEye.Photo.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CQRSlite.Domain;
    using Dawn;
    using EagleEye.Photo.Domain.Events;
    using JetBrains.Annotations;

    internal class Photo : AggregateRoot
    {
        private const int Sha256ByteSize = 256 / 8;
        [NotNull] private readonly Dictionary<string, ulong> photoHashes;
        [NotNull] private readonly List<string> tags;
        [NotNull] private readonly List<string> persons;
        private DateTime? dateTimeTaken;

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
            Guard.Argument(id, nameof(id)).NotEqual(Guid.Empty);
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            Guard.Argument(mimeType, nameof(mimeType)).NotNull().NotWhiteSpace();
            Guard.Argument(fileSha256, nameof(fileSha256)).NotNull().Count(Sha256ByteSize);

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

        [CanBeNull]
        public Location Location => location;

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

        /// <summary>Set location of photo object.</summary>
        /// <param name="countryCode">CountryCode; see <see href="https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes"/>.</param>
        /// <param name="countryName">Country name.</param>
        /// <param name="state">State.</param>
        /// <param name="city">City.</param>
        /// <param name="subLocation">Sub location.</param>
        /// <param name="longitude">GPS longitude.</param>
        /// <param name="latitude">GPS latitude.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="longitude"/> or <paramref name="latitude"/> is <c>null</c>.</exception>
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

            ApplyChange(new LocationClearedFromPhoto(Id));
        }

        public void SetDateTimeTaken(DateTime dateTime, TimestampPrecision precision)
        {
            // todo check
            ApplyChange(new DateTimeTakenChanged(Id, dateTime, precision));
        }

        public void UpdateFileHash([NotNull] byte[] fileHash)
        {
            Guard.Argument(fileHash, nameof(fileHash)).NotNull().NotEmpty();

            if (!this.fileHash.SequenceEqual(fileHash))
                ApplyChange(new FileHashUpdated(Id, fileHash));
        }

        public void UpdatePhotoHash([NotNull] string hashIdentifier, ulong fileHash)
        {
            Guard.Argument(hashIdentifier, nameof(hashIdentifier)).NotNull().NotWhiteSpace();

            if (!photoHashes.ContainsKey(hashIdentifier))
                ApplyChange(new PhotoHashAdded(Id, hashIdentifier, fileHash));
            else if (photoHashes[hashIdentifier] != fileHash)
                ApplyChange(new PhotoHashUpdated(Id, hashIdentifier, fileHash));
        }

        public void ClearPhotoHash([NotNull] string hashIdentifier)
        {
            Guard.Argument(hashIdentifier, nameof(hashIdentifier)).NotNull().NotWhiteSpace();

            if (photoHashes.ContainsKey(hashIdentifier))
                ApplyChange(new PhotoHashCleared(Id, hashIdentifier));
        }

        [UsedImplicitly]
        private void Apply([NotNull] FileHashUpdated e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            fileHash = e.Hash.ToArray();
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashAdded e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            photoHashes.Add(e.HashIdentifier, e.Hash);
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashUpdated e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            photoHashes[e.HashIdentifier] = e.Hash;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoHashCleared e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            photoHashes.Remove(e.HashIdentifier);
        }

        [UsedImplicitly]
        private void Apply([NotNull] LocationClearedFromPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            location = null;
        }

        [UsedImplicitly]
        private void Apply([NotNull] LocationSetToPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            location = e.Location;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PhotoCreated e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            Id = e.Id;
            filename = e.FileName;
            fileHash = e.FileHash;
        }

        [UsedImplicitly]
        private void Apply([NotNull] PersonsAddedToPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            persons.AddRange(e.Persons);
        }

        [UsedImplicitly]
        private void Apply([NotNull] PersonsRemovedFromPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            foreach (var t in e.Persons)
                persons.Remove(t);
        }

        [UsedImplicitly]
        private void Apply([NotNull] TagsAddedToPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            tags.AddRange(e.Tags);
        }

        [UsedImplicitly]
        private void Apply([NotNull] TagsRemovedFromPhoto e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            foreach (var t in e.Tags)
                tags.Remove(t);
        }

        [UsedImplicitly]
        private void Apply([NotNull] DateTimeTakenChanged e)
        {
            Guard.Argument(e, nameof(e)).NotNull();

            dateTimeTaken = e.DateTimeTaken;
            dateTimeTakenPrecision = e.Precision;
        }
    }
}
