namespace EagleEye.Photo.Domain.Test.Aggregates
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
    using EagleEye.Photo.Domain.Events;
    using FluentAssertions;
    using Xunit;

    public class PhotoTest
    {
        private Guid guid;
        private string filename;
        private string mimeType;
        private byte[] fileSha;

        public PhotoTest()
        {
            guid = Guid.NewGuid();
            filename = "c:/a/b/c.jpg";
            mimeType = "image/jpg";
            fileSha = new byte[32];
        }

        [Fact]
        public void ConstructingPhoto_ShouldSetIdAndVersion_WhenCalledWithCorrectParameters()
        {
            // arrange

            // act
            var result = new Photo(guid, filename, mimeType, fileSha);

            // assert
            result.Id.Should().Be(guid);
            result.Version.Should().Be(0);
        }

        [Fact]
        public void ConstructingPhoto_ShouldThrow_WhenCalledWithIncorrectId()
        {
            // arrange
            guid = Guid.Empty;

            // act
            Action act = () => _ = new Photo(guid, filename, mimeType, fileSha);

            // assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConstructingPhoto_ShouldThrow_WhenCalledWithIncorrectFilename()
        {
            // arrange
            filename = string.Empty;

            // act
            Action act = () => _ = new Photo(guid, filename, mimeType, fileSha);

            // assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConstructingPhoto_ShouldThrow_WhenCalledWithIncorrectMimetype()
        {
            // arrange
            mimeType = string.Empty;

            // act
            Action act = () => _ = new Photo(guid, filename, mimeType, fileSha);

            // assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConstructingPhoto_ShouldThrow_WhenCalledWithIncorrectFileHash()
        {
            // arrange
            fileSha = new byte[31];

            // act
            Action act = () => _ = new Photo(guid, filename, mimeType, fileSha);

            // assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AddPersons_ShouldDoNothing_WhenNoValueProvided()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.AddPersons();

            // assert
            sut.Persons.Should().BeEmpty();
        }

        [Fact]
        public void AddPersons_ShouldAddNewPersonToNamesOfPhoto_WhenNameWasNotAlreadyAdded()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.AddPersons("Donald Duck");

            // assert
            sut.Persons.Should().BeEquivalentTo("Donald Duck");
        }

        [Fact]
        public void AddPersons_ShouldNotAddNewPersonToNamesOfPhoto_WhenNameWasAlreadyPresent()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddPersons("Donald Duck");

            // act
            sut.AddPersons("Donald Duck");

            // assert
            sut.Persons.Should().BeEquivalentTo("Donald Duck");
        }

        [Fact]
        public void AddPersons_ShouldDoNothing_WhenNullPersonWasAdded()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddPersons("Donald Duck");

            // act
            sut.AddPersons(null);

            // assert
            sut.Persons.Should().BeEquivalentTo("Donald Duck");
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void AddPersons_ShouldDoNothing_WhenEmptyStringPersonWasAdded(string personName)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddPersons("Donald Duck");

            // act
            sut.AddPersons(personName);

            // assert
            sut.Persons.Should().BeEquivalentTo("Donald Duck");
        }

        [Fact]
        public void AddPersons_ShouldHandleMultipleNamesAtOnce()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddPersons("Donald Duck");

            // act
            sut.AddPersons("Huey", "Donald Duck", null, "Dewey", "  ", "Louie");

            // assert
            sut.Persons.Should().BeEquivalentTo("Donald Duck", "Huey", "Dewey", "Louie");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Donald Duck")]
        public void RemovePersons_ShouldDoNothing_WhenNoPersonsAreInPlace(string person)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.RemovePersons(person);

            // assert
            sut.Persons.Should().BeEmpty();
        }

        [Fact]
        public void RemovePersons_ShouldDoNothing_WhenPersonsArrayIsNull()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.RemovePersons(null);

            // assert
            sut.Persons.Should().BeEmpty();
        }

        [Fact]
        public void RemovePersons_ShouldRemovePersons_WhenPersonExistsInPhoto()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddPersons("Donald Duck", "Huey", "Dewey");

            // act
            sut.RemovePersons("Donald Duck", "Dewey", "Dummy");

            // assert
            sut.Persons.Should().BeEquivalentTo("Huey");
        }

        [Fact]
        public void AddTags_ShouldDoNothing_WhenNoValueProvided()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.AddTags();

            // assert
            sut.Tags.Should().BeEmpty();
        }

        [Fact]
        public void AddTags_ShouldAddNewTagToPhoto_WhenTagWasNotAlreadyAdded()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.AddTags("Holiday");

            // assert
            sut.Tags.Should().BeEquivalentTo("Holiday");
        }

        [Fact]
        public void AddTags_ShouldNotAddNewTagToPhoto_WhenTagWasAlreadyPresent()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddTags("Holiday");

            // act
            sut.AddTags("Holiday");

            // assert
            sut.Tags.Should().BeEquivalentTo("Holiday");
        }

        [Fact]
        public void AddTags_ShouldDoNothing_WhenNullTagWasAdded()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddTags("Holiday");

            // act
            sut.AddTags(null);

            // assert
            sut.Tags.Should().BeEquivalentTo("Holiday");
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void AddTags_ShouldDoNothing_WhenEmptyStringTagWasAdded(string tag)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddTags("Holiday");

            // act
            sut.AddTags(tag);

            // assert
            sut.Tags.Should().BeEquivalentTo("Holiday");
        }

        [Fact]
        public void AddTags_ShouldHandleMultipleTagsAtOnce()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddTags("Holiday");

            // act
            sut.AddTags("Summer", "Holiday", null, "New York", "  ", "Disney");

            // assert
            sut.Tags.Should().BeEquivalentTo("Holiday", "Summer", "New York", "Disney");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Holiday")]
        public void RemoveTags_ShouldDoNothing_WhenNoTagsAreInPlace(string tag)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.RemoveTags(tag);

            // assert
            sut.Tags.Should().BeEmpty();
        }

        [Fact]
        public void RemoveTags_ShouldDoNothing_WhenTagsArrayIsNull()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // act
            sut.RemoveTags(null);

            // assert
            sut.Tags.Should().BeEmpty();
        }

        [Fact]
        public void RemoveTags_ShouldRemoveTags_WhenTagExistsInPhoto()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.AddTags("Holiday", "Summer", "Beach");

            // act
            sut.RemoveTags("Holiday", "Summer", "Dummy");

            // assert
            sut.Tags.Should().BeEquivalentTo("Beach");
        }

        [Fact]
        public void Location_ShouldBeEmpty_WhenConstructingPhoto()
        {
            // act
            var sut = new Photo(guid, filename, mimeType, fileSha);

            // assert
            sut.Location.Should().BeNull();
        }

        [Theory]
        [InlineData("NL", "Netherlands", "Utrecht", "utrecht", "Central", 12, 31)]
        [InlineData("NL", "Netherlands", "Utrecht", "utrecht", "Central", null, null)]
        public void SetLocation_ShouldUpdateLocation_WhenNoLocationWasPresent(
            string countryCode,
            string countryName,
            string state,
            string city,
            string subLocation,
            float? longitude,
            float? latitude)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            _ = sut.FlushUncommittedChanges();

            // act
            sut.SetLocation(countryCode, countryName, state, city, subLocation, longitude, latitude);

            // assert
            var expectedLocation = new Location(countryCode, countryName, state, city, subLocation, longitude, latitude);
            sut.Location.Should().BeEquivalentTo(expectedLocation);
            sut.GetUncommittedChanges().Should().BeEquivalentTo(new LocationSetToPhoto(guid, expectedLocation));
        }

        [Theory]
        [InlineData("NL", "Netherlands", "Utrecht", "utrecht", "Central", null, 1)]
        [InlineData("NL", "Netherlands", "Utrecht", "utrecht", "Central", 1, null)]
        public void SetLocation_ShouldThrow_WhenLongitudeLatitudeWasInvalid(
            string countryCode,
            string countryName,
            string state,
            string city,
            string subLocation,
            float? longitude,
            float? latitude)
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            _ = sut.FlushUncommittedChanges();

            // act
            Action act = () => sut.SetLocation(countryCode, countryName, state, city, subLocation, longitude, latitude);

            // assert
            act.Should().Throw<ArgumentException>();
            sut.Location.Should().BeNull();
            sut.GetUncommittedChanges().Should().BeEmpty();
        }

        [Fact]
        public void ClearLocationData_ShouldDoNothing_WhenNoLocationWasSet()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            _ = sut.FlushUncommittedChanges();

            // act
            sut.ClearLocationData();

            // assert
            sut.Location.Should().BeNull();
            sut.GetUncommittedChanges().Should().BeEmpty($"No events should have been fired.");
        }

        [Fact]
        public void ClearLocationData_ShouldClearLocation_WhenLocationWasSet()
        {
            // arrange
            var sut = new Photo(guid, filename, mimeType, fileSha);
            sut.SetLocation("a", "b", "c", "d", "e", 1, 2);
            _ = sut.FlushUncommittedChanges();

            // act
            sut.ClearLocationData();

            // assert
            sut.Location.Should().BeNull();
            sut.GetUncommittedChanges().Should().BeEquivalentTo(new LocationClearedFromPhoto(guid));
        }
    }
}
