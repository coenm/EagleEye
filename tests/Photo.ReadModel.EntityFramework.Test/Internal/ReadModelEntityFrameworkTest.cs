namespace Photo.ReadModel.EntityFramework.Test.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework;
    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;
    using FakeItEasy;
    using FluentAssertions;
    using Photo.ReadModel.EntityFramework.Test.Internal.Helpers;
    using Xunit;

    using Sut = EagleEye.Photo.ReadModel.EntityFramework.Internal.ReadModelEntityFramework;

    public class ReadModelEntityFrameworkTest
    {
        private readonly ReadModelEntityFramework sut;
        private readonly IEagleEyeRepository repository;
        private readonly DateTime dt;

        public ReadModelEntityFrameworkTest()
        {
            dt = new DateTime(1993, 3, 5, 23, 52, 11);

            repository = A.Fake<IEagleEyeRepository>();
            sut = new ReadModelEntityFramework(repository);
        }

        [Fact]
        public async Task GetAllPhotosAsync_ShouldCallRepository()
        {
            // arrange

            // act
            _ = await sut.GetAllPhotosAsync();

            // assert
            A.CallTo(() => repository.GetAllAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetAllPhotosAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // arrange
            A.CallTo(() => repository.GetAllAsync()).Returns(Task.FromResult(null as List<Photo>));

            // act
            var result = await sut.GetAllPhotosAsync();

            // assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAllPhotosAsync_ShouldRethrow_WhenRepositoryThrowsAsync()
        {
            // arrange
            A.CallTo(() => repository.GetAllAsync())
                .ReturnsLazily(
                    async call =>
                    {
                        await Task.Yield();
                        throw new Exception("This is a test exception");
                    });

            // act
            Func<Task> act = async () => _ = await sut.GetAllPhotosAsync();

            // assert
            act.Should().Throw<Exception>().WithMessage("This is a test exception");
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Sut method can return null")]
        public async Task GetPhotoByGuidAsync_ShouldCallRepository()
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
                Tags = TestHelpers.CreateTags("zoo", "holiday"),
                DateTimeTaken = dt.AddDays(2),
            };
            A.CallTo(() => repository.GetByIdAsync(guid))
                .Returns(Task.FromResult(photo));

            // act
            _ = await sut.GetPhotoByGuidAsync(guid);

            // assert
            A.CallTo(() => repository.GetByIdAsync(guid)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Sut method can return null")]
        public async Task GetPhotoByGuidAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => repository.GetByIdAsync(guid)).Returns(Task.FromResult(null as Photo));

            // act
            var result = await sut.GetPhotoByGuidAsync(guid);

            // assert
            result.Should().BeNull();
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Sut method can return null")]
        public void GetPhotoByGuidAsync_ShouldRethrow_WhenRepositoryThrowsAsync()
        {
            // arrange
            var guid = Guid.NewGuid();
            A.CallTo(() => repository.GetByIdAsync(guid))
                .ReturnsLazily(
                    async call =>
                    {
                        await Task.Yield();
                        throw new Exception("This is a test exception");
                    });

            // act
            Func<Task> act = async () => _ = await sut.GetPhotoByGuidAsync(guid);

            // assert
            act.Should().Throw<Exception>().WithMessage("This is a test exception");
        }

        [Fact]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Sut method can return null")]
        public async Task GetPhotoByGuidAsync_ShouldReturnMappedResult_WhenRepositoryReturnsPhoto()
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
                Tags = TestHelpers.CreateTags("zoo", "holiday"),
                DateTimeTaken = dt.AddDays(2),
            };
            A.CallTo(() => repository.GetByIdAsync(guid))
                .ReturnsLazily(
                    async call =>
                    {
                        await Task.Yield();
                        return photo;
                    });

            // act
            var result = await sut.GetPhotoByGuidAsync(guid);

            // assert
            var expectedResult = new EagleEye.Photo.ReadModel.EntityFramework.Interface.Model.Photo(
                guid,
                "dummy.jpg",
                "image/jpeg",
                new byte[] { 0x01 },
                new[] { "zoo", "holiday" },
                new string[0],
                null,
                dt.AddDays(2),
                1234);
            result.Should().BeEquivalentTo(expectedResult);
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
            var location = new Location
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
                Tags = TestHelpers.CreateTags("zoo", "holiday"),
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
                new[] { "zoo", "holiday" },
                new string[0],
                null,
                dt.AddDays(2),
                1234);

            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
