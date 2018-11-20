namespace EagleEye.Core.ReadModel.Model
{
    using System.Collections.Generic;

    using Helpers.Guards;
    using JetBrains.Annotations;

    public class Photo
    {
        internal Photo(
            [NotNull] string filename,
            [NotNull] byte[] fileSha256,
            [NotNull] IReadOnlyList<string> tags,
            [NotNull] IReadOnlyList<string> persons,
            [CanBeNull] Location location,
            int version)
        {
            DebugGuard.NotNull(filename, nameof(filename));
            DebugGuard.NotNull(fileSha256, nameof(fileSha256));
            DebugGuard.NotNull(tags, nameof(tags));
            DebugGuard.NotNull(persons, nameof(persons));

            Filename = filename;
            FileSha256 = fileSha256;
            Tags = tags;
            Persons = persons;
            Location = location;
            Version = version;
        }

        public string Filename { get; }

        public byte[] FileSha256 { get; }

        public IReadOnlyList<string> Tags { get; }

        public IReadOnlyList<string> Persons { get; }

        public Location Location { get; }

        public int Version { get; }
    }
}
