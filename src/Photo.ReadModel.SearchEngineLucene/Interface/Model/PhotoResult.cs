namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Interface.Model
{
    using System;
    using System.Collections.Generic;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    public class PhotoResult : PhotoIdResult
    {
        internal PhotoResult(
            Guid id,
            [NotNull] string filename,
            string mimeType,
            [NotNull] IReadOnlyList<string> tags,
            [NotNull] IReadOnlyList<string> persons,
            [CanBeNull] Location location,
            int version,
            float score)
            : base(id, score)
        {
            DebugHelpers.Guards.Guard.NotNull(filename, nameof(filename));
            DebugHelpers.Guards.Guard.NotNull(tags, nameof(tags));
            DebugHelpers.Guards.Guard.NotNull(persons, nameof(persons));

            Filename = filename;
            MimeType = mimeType;
            Tags = tags;
            Persons = persons;
            Location = location;
            Version = version;
        }

        public string Filename { get; }

        public string MimeType { get; }

        public IReadOnlyList<string> Tags { get; }

        public IReadOnlyList<string> Persons { get; }

        public Location Location { get; }

        public int Version { get; }
    }
}
