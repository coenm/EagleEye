namespace EagleEye.Photo.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;

    using CQRSlite.Snapshotting;
    using EagleEye.Photo.Domain.Aggregates.SnapshotDtos;

    internal partial class Photo : SnapshotAggregateRoot<PhotoAggregateSnapshot>
    {
        protected override PhotoAggregateSnapshot CreateSnapshot()
        {
            return new PhotoAggregateSnapshot
                   {
                       PhotoHashes = photoHashes,
                       Tags = tags,
                       Persons = persons,
                       DateTimeTaken = dateTimeTaken,
                       Location = location == null
                                      ? null
                                      : new LocationSnapshot
                                        {
                                            CountryCode = location.CountryCode,
                                            CountryName = location.CountryName,
                                            State = location.State,
                                            City = location.City,
                                            SubLocation = location.SubLocation,
                                            Longitude = location.Longitude,
                                            Latitude = location.Latitude,
                                        },
                       Filename = filename,
                       FileHash = fileHash,
                   };
        }

        protected override void RestoreFromSnapshot(PhotoAggregateSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            if (snapshot.PhotoHashes != null)
            {
                foreach (KeyValuePair<string, ulong> snapshotPhotoHash in snapshot.PhotoHashes)
                {
                    photoHashes.Add(snapshotPhotoHash.Key, snapshotPhotoHash.Value);
                }
            }

            if (snapshot.Tags != null)
                tags.AddRange(snapshot.Tags);

            if (snapshot.Persons != null)
                persons.AddRange(snapshot.Persons);

            filename = snapshot.Filename;
            fileHash = snapshot.FileHash;
            dateTimeTaken = snapshot.DateTimeTaken;

            if (snapshot.Location != null)
            {
                location = new Location(
                                        snapshot.Location.CountryCode,
                                        snapshot.Location.CountryName,
                                        snapshot.Location.State,
                                        snapshot.Location.City,
                                        snapshot.Location.SubLocation,
                                        snapshot.Location.Longitude,
                                        snapshot.Location.Latitude);
            }
        }
    }
}
