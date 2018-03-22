namespace EagleEye.Picasa.Test.Picasa
{
    using EagleEye.Picasa.Picasa;

    using FluentAssertions;

    using Xunit;

    public class FileWithPersonsTest
    {
        private FileWithPersons _sut;

        public FileWithPersonsTest()
        {
            _sut = new FileWithPersons("file1.jpg", "Bob");
        }

        [Fact]
        public void Ctor_ShouldSetDataTest()
        {
            _sut.Filename.Should().Be("file1.jpg");
            _sut.Persons.Should().BeEquivalentTo("Bob");
        }

        [Fact]
        public void AddPerson_ShouldAddNameToPersonsTest()
        {
            // arrange

            // act
            _sut.AddPerson("Carl");

            // assert
            _sut.Persons.Should().BeEquivalentTo("Bob", "Carl");
        }

        [Fact]
        public void AddPerson_ThatAlreadyExists_ShouldNotAddPersonAgainTest()
        {
            // arrange

            // act
            _sut.AddPerson("Bob");

            // assert
            _sut.Persons.Should().BeEquivalentTo("Bob");
        }

        [Fact]
        public void ToString_ReturnsFileOnlyTest()
        {
            // arrange
            _sut = new FileWithPersons("file.jpg");

            // act
            var result = _sut.ToString();

            // assert
            result.Should().Be("file.jpg has no persons.");
        }

        [Fact]
        public void ToString_ReturnsFileWithPersonsTest()
        {
            // arrange
            _sut = new FileWithPersons("file.jpg", "Bob", "Alice");

            // act
            var result = _sut.ToString();

            // assert
            result.Should().Be("file.jpg has persons: Bob, Alice");
        }
    }
}