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
            sut = new FileWithPersons("file1.jpg", new PicasaPerson("Bob"));
        }

        [Fact]
        public void Ctor_ShouldSetDataTest()
        {
            sut.Filename.Should().Be("file1.jpg");
            sut.Persons.Should().BeEquivalentTo(new PicasaPerson("Bob"));
        }

        [Fact]
        public void AddPerson_ShouldAddNameToPersonsTest()
        {
            // arrange

            // act
            sut.AddPerson(new PicasaPerson("Carl"));

            // assert
            sut.Persons.Should().BeEquivalentTo(new PicasaPerson("Bob"), new PicasaPerson("Carl"));
        }

        [Fact]
        public void AddPerson_ThatAlreadyExists_ShouldNotAddPersonAgainTest()
        {
            // arrange

            // act
            sut.AddPerson(new PicasaPerson("Bob"));

            // assert
            sut.Persons.Should().BeEquivalentTo(new PicasaPerson("Bob"));
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
            sut = new FileWithPersons("file.jpg", new PicasaPerson("Bob"), new PicasaPerson("Alice"));

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("file.jpg has persons: Bob, Alice");
        }
    }
}
