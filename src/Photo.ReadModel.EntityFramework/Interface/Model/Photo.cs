namespace EagleEye.Photo.ReadModel.EntityFramework.Interface.Model
{
    using System;
    using System.Collections.Generic;

    using Helpers.Guards; using Dawn;
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
            DebugHelpers.Guards.Guard.NotNull(filename, nameof(filename));
            DebugHelpers.Guards.Guard.NotNull(fileSha256, nameof(fileSha256));
            DebugHelpers.Guards.Guard.NotNull(tags, nameof(tags));
            DebugHelpers.Guards.Guard.NotNull(persons, nameof(persons));

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
