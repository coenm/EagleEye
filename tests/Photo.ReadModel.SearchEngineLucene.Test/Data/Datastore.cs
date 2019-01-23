namespace Photo.ReadModel.SearchEngineLucene.Test.Data
{
    using System;
    using System.Collections.Generic;

    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;

    internal static class DataStore
    {
        public static Photo File001 => new Photo
        {
            Id = Guid.Parse("FE364228-A066-4A78-9C29-60C0B2077451"),
            FileName = "a/b/c/file.jpg",
            FileMimeType = "image/jpeg",
            DateTimeTaken = new Timestamp(new DateTime(2001, 4, 1, 0, 0, 0), TimestampPrecision.Month),
            LocationCity = "New York",
            LocationState = "New York",
            LocationCountryName = "United States of America",
            LocationSubLocation = "Ground zero",
            LocationCountryCode = "USA",
            LocationLatitude = 2.233F,
            LocationLongitude = -21.234F,
            Persons = new List<string>
            {
                "Alice",
                "Bob",
            },
            Tags = new List<string>
            {
                "Vacation",
                "Summer",
            },
            Version = 12,
        };

        public static PhotoSearchResult MediaResult001(float score)
        {
            return new PhotoSearchResult(score)
            {
                Id = Guid.Parse("FE364228-A066-4A78-9C29-60C0B2077451"),
                FileName = "a/b/c/file.jpg",
                FileMimeType = "image/jpeg",
                DateTimeTaken = new Timestamp(new DateTime(2001, 4, 1, 0, 0, 0), TimestampPrecision.Month),
                LocationCity = "New York",
                LocationState = "New York",
                LocationCountryName = "United States of America",
                LocationSubLocation = "Ground zero",
                LocationCountryCode = "USA",
                LocationLatitude = 2.233F,
                LocationLongitude = -21.234F,
                Persons = new List<string>
                {
                    "Alice",
                    "Bob",
                },
                Tags = new List<string>
                {
                    "Vacation",
                    "Summer",
                },
                Version = 12,
            };
        }
    }
}
