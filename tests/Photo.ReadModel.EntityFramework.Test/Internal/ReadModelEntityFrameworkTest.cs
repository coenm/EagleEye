namespace Photo.ReadModel.EntityFramework.Test.Internal
{
    using System;
    using System.Collections.Generic;
    using Castle.Components.DictionaryAdapter;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;
    using Xunit.Sdk;
    using Sut = EagleEye.Photo.ReadModel.EntityFramework.Internal.ReadModelEntityFramework;

    public class ReadModelEntityFrameworkTest
    {
        private ReadModelEntityFramework sut;
        private readonly IEagleEyeRepository repository;
        private readonly DateTime dt;

        public ReadModelEntityFrameworkTest()
        {
            dt = new DateTime(1993, 3, 5, 23,52, 11);

            repository = A.Fake<IEagleEyeRepository>();
            sut = new ReadModelEntityFramework(repository);
        }

        [Fact]
        public void MapLocation_ShouldReturnNull_WhenInputIsNull()
        {
            // arrange

            // act
            var result = Sut.MapLocation(null);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void MapLocation_ShouldMap_WhenInputIsNotNull()
        {
            // arrange
            var location = new Location()
            {
                Id = Guid.NewGuid(),
                CountryName = "USA",
                State = "NY",
                Longitude = 12333,
                SubLocation = "Ground Zero",
                Latitude = 4444,
                CountryCode = "US",
                City = "New York",
            };

            // act
            var result = Sut.MapLocation(location);

            // assert
            var expectedResult = new EagleEye.Photo.ReadModel.EntityFramework.Interface.Model.Location(
              "US",
              "USA",
              "New York",
              "NY",
              "Ground Zero");
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void MapPhoto()
        {
            // arrange
            var guid = Guid.NewGuid();
            var photo = new Photo
            {
                Id = guid,
                FileMimeType = "image/jpeg",
                Filename = "dummy.jpg",
                Version = 1234,
                FileSha256 = new byte[] { 0x01 },
                EventTimestamp = dt,
                Location = null,
                Tags = new List<Tag>
                {
                    new Tag { Value = "zoo" },
                    new Tag { Value = "holiday" },
                },
                DateTimeTaken = dt.AddDays(2),
            };

            // act
            var result = Sut.MapPhoto(photo);

            // assert
            var expectedResult = new EagleEye.Photo.ReadModel.EntityFramework.Interface.Model.Photo(
                guid,
                "dummy.jpg",
                "image/jpeg",
                new byte[] { 0x01 },
                new[] { "zoo", "holiday"},
                new string[0],
                null,
                dt.AddDays(2),
                1234);

            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
