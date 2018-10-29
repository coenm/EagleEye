namespace EagleEye.Picasa.Test.Picasa
{
    using EagleEye.Picasa.Picasa;

    using FluentAssertions;

    using Xunit;

    public class FileWithPersonsTest
    {
        private FileWithPersons sut;

        public FileWithPersonsTest()
        {
            sut = new FileWithPersons("file1.jpg", "Bob");
        }

        [Fact]
        public void Ctor_ShouldSetDataTest()
        {
            sut.Filename.Should().Be("file1.jpg");
            sut.Persons.Should().BeEquivalentTo("Bob");
        }

        [Fact]
        public void AddPerson_ShouldAddNameToPersonsTest()
        {
            // arrange

            // act
            sut.AddPerson("Carl");

            // assert
            sut.Persons.Should().BeEquivalentTo("Bob", "Carl");
        }

        [Fact]
        public void AddPerson_ThatAlreadyExists_ShouldNotAddPersonAgainTest()
        {
            // arrange

            // act
            sut.AddPerson("Bob");

            // assert
            sut.Persons.Should().BeEquivalentTo("Bob");
        }

        [Fact]
        public void ToString_ReturnsFileOnlyTest()
        {
            // arrange
            sut = new FileWithPersons("file.jpg");

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("file.jpg has no persons.");
        }

        [Fact]
        public void ToString_ReturnsFileWithPersonsTest()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", "Bob", "Alice");

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("file.jpg has persons: Bob, Alice");
        }
    }
}
