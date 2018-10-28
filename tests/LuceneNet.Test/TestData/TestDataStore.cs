namespace EagleEye.LuceneNet.Test.TestData
{
    using System;
    using System.Collections.Generic;

    using EagleEye.LuceneNet.Test.Data;

    public static class TestDataStore
    {
        public static MediaObject File1 { get; } = new MediaObject
                                                       {
                                                           Location =
                                                               {
                                                                   State = "New York",
                                                                   Coordinate = new Coordinate
                                                                                    {
                                                                                        Latitude = 40.736072f,
                                                                                        Longitude = -73.994293f,
                                                                                    },
                                                                   CountryName = "United States",
                                                                   CountryCode = "USA",
                                                                   SubLocation = "Union Square",
                                                                   City = "New York",
                                                               },
                                                           Tags = new List<string>
                                                                      {
                                                                          "Vaction",
                                                                          "Summmer",
                                                                      },
                                                           Persons = new List<string>
                                                                         {
                                                                             "James Bond",
                                                                             "MoneyPenny",
                                                                         },
                                                           DateTimeTaken = new Timestamp
                                                                               {
                                                                                   Precision = TimestampPrecision.Month,
                                                                                   Value = new DateTime(2010, 3, 1, 1, 1, 1, 1),
                                                                               },
                                                           FileInformation = new FileInformation
                                                                                 {
                                                                                     Type = "image/jpeg",
                                                                                     Filename = "file1.jpg",
                                                                                 },
                                                       };
    }
}
