namespace EagleEye.Photo.Domain.Aggregates.SnapshotDtos
{
    using System.Collections.Generic;

    using CQRSlite.Snapshotting;

    internal class PhotoAggregateSnapshot : Snapshot
    {
        public Dictionary<string, ulong> PhotoHashes { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Persons { get; set; }

        public Timestamp DateTimeTaken { get; set; }

        public LocationSnapshot Location { get; set; }

        public string Filename { get; set; }

        public byte[] FileHash { get; set; }
    }
}
