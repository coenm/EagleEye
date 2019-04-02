namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Interface.Model
{
    using System;
    using System.Collections.Generic;

    using Dawn;
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
            Guard.Argument(filename, nameof(filename)).NotNull();
            Guard.Argument(tags, nameof(tags)).NotNull();
            Guard.Argument(persons, nameof(persons)).NotNull();

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
