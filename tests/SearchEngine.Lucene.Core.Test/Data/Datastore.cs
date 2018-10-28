namespace SearchEngine.Lucene.Core.Test.Data
{
    using System;
    using System.Collections.Generic;

    using SearchEngine.Interface.Commands.ParameterObjects;
    using SearchEngine.LuceneNet.Core.Index;

    public static class DataStore
    {
        public static MediaObject File001 => new MediaObject
                                                 {
                                                     DateTimeTaken = new Timestamp
                                                                         {
                                                                             Value = new DateTime(2001, 4, 1, 0, 0, 0),
                                                                             Precision = TimestampPrecision.Month,
                                                                         },
                                                     Location = new Location
                                                                    {
                                                                        City = "New York",
                                                                        State = "New York",
                                                                        CountryName = "United States of America",
                                                                        SubLocation = "Ground zero",
                                                                        CountryCode = "USA",
                                                                        Coordinate = new Coordinate
                                                                                         {
                                                                                             Latitude = (float)2.233,
                                                                                             Longitude = (float)-21.234,
                                                                                         }
                                                                    },
                                                     Persons = new List<string>
                                                                   {
                                                                       "Alice",
                                                                       "Bob"
                                                                   },
                                                     Tags = new List<string>
                                                                {
                                                                    "Vacation",
                                                                    "Summer"
                                                                },
                                                     FileInformation = new FileInformation
                                                                           {
                                                                               Type = "image/jpeg",
                                                                               Filename = "a/b/c/file.jpg"
                                                                           }
                                                 };

        public static MediaResult MediaResult001(float score)
        {
            return new MediaResult(score)
            {
                DateTimeTaken = new Timestamp
                {
                    Value = new DateTime(2001, 4, 1, 0, 0, 0),
                    Precision = TimestampPrecision.Month,
                },
                Location = new Location
                {
                    City = "New York",
                    State = "New York",
                    CountryName = "United States of America",
                    SubLocation = "Ground zero",
                    CountryCode = "USA",
                    Coordinate = new Coordinate
                    {
                        Latitude = (float)2.233,
                        Longitude = (float)-21.234,
                    }
                },
                Persons = new List<string>
                {
                    "Alice",
                    "Bob"
                },
                Tags = new List<string>
                {
                    "Vacation",
                    "Summer"
                },
                FileInformation = new FileInformation
                {
                    Type = "image/jpeg",
                    Filename = "a/b/c/file.jpg"
                }
            };
        }
    }
}
