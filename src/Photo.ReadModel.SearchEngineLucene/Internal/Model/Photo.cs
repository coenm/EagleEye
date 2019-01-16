namespace Photo.ReadModel.SearchEngineLucene.Internal.Model
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

    internal class Photo
    {
        public Guid Id { get; set; }

        public int Version { get; set; }

        public string FileName { get; set; }

        public string FileMimeType { get; set; }

        public List<string> Persons { get; set; }

        public List<string> Tags { get; set; }

        public string LocationCountryCode { get; set; }

        public string LocationCountryName { get; set; }

        public string LocationCity { get; set; }

        public string LocationState { get; set; }

        public string LocationSubLocation { get; set; }

        public float? LocationLatitude { get; set; }

        public float? LocationLongitude { get; set; }

        [CanBeNull]
        public Timestamp DateTimeTaken { get; set; }
    }
}
