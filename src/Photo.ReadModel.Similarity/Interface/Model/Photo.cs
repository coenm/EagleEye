namespace Photo.ReadModel.Similarity.Interface.Model
{
    using System;
    using System.Collections.Generic;

    using Helpers.Guards;

    using JetBrains.Annotations;

    public class Photo
    {
        internal Photo(
            Guid id,
            [NotNull] string filename,
            [NotNull] string fileMimeType,
            [NotNull] byte[] fileSha256,
            [NotNull] IReadOnlyList<string> tags,
            [NotNull] IReadOnlyList<string> persons,
            [CanBeNull] Location location,
            DateTime? taken,
            int version)
        {
            DebugGuard.NotNull(filename, nameof(filename));
            DebugGuard.NotNull(fileSha256, nameof(fileSha256));
            DebugGuard.NotNull(tags, nameof(tags));
            DebugGuard.NotNull(persons, nameof(persons));

            Id = id;
            Filename = filename;
            FileMimeType = fileMimeType;
            FileSha256 = fileSha256;
            Tags = tags;
            Persons = persons;
            Location = location;
            Taken = taken;
            Version = version;
        }

        public Guid Id { get; }

        public string Filename { get; }

        public string FileMimeType { get; }

        public byte[] FileSha256 { get; }

        public IReadOnlyList<string> Tags { get; }

        public IReadOnlyList<string> Persons { get; }

        public Location Location { get; }

        public DateTime? Taken { get; }

        public int Version { get; }
    }
}
