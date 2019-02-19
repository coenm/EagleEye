namespace EagleEye.Photo.Domain.Test.Aggregates
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
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
    }
}
