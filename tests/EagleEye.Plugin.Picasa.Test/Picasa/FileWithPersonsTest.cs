namespace EagleEye.Picasa.Test.Picasa
{
    using System.Collections.Generic;
    using System.Linq;

    using EagleEye.Picasa.Picasa;
    using FluentAssertions;
    using Xunit;

    public class FileWithPersonsTest
    {
        private FileWithPersons sut;

        public FileWithPersonsTest()
        {
            sut = new FileWithPersons("file1.jpg", new PicasaPersonLocation("Bob"));
        }

        [Fact]
        public void Ctor_ShouldSetDataTest()
        {
            sut.Filename.Should().Be("file1.jpg");
            sut.Persons.Should().BeEquivalentTo(new PicasaPersonLocation("Bob"));
        }

        [Fact]
        public void AddPerson_ShouldAddNameToPersonsTest()
        {
            // arrange

            // act
            sut.AddPerson(new PicasaPersonLocation("Carl"));

            // assert
            sut.Persons.Should().BeEquivalentTo(new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Carl"));
        }

        [Fact]
        public void AddPerson_ThatAlreadyExists_ShouldNotAddPersonAgainTest()
        {
            // arrange

            // act
            sut.AddPerson(new PicasaPersonLocation("Bob"));

            // assert
            sut.Persons.Should().BeEquivalentTo(new PicasaPersonLocation("Bob"));
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
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("file.jpg has persons: Bob, Alice");
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenNull()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(null);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherFilenameValues()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(sut = new FileWithPersons("otherfile.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice")));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherPersonNameValues()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob1"), new PicasaPersonLocation("Alice")));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenPersonLocationHasRegion()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob", new RelativeRegion(1, 2, 3, 4)), new PicasaPersonLocation("Alice")));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToSameObjectReference()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(sut);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToOtherFileWithPersonsWithSameValues()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice")));

            // assert
            sut.Should().Be(new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice")));
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenComparedToOtherObjectWithSameValues()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));
            object objectFileWithPerson = (object)new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));

            // act
            var result = sut.Equals(objectFileWithPerson);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenComparedToString()
        {
            // arrange
            sut = new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice"));
            object notePicasaPersonObject = new string('a', 100);

            // act
            var result = sut.Equals(notePicasaPersonObject);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldDependOnAllProperties()
        {
            // arrange
            var suts = new List<FileWithPersons>
                       {
                           new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice")),
                           new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice", new RelativeRegion(1, 2, 3, 4))),
                           new FileWithPersons("file.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice", new RelativeRegion(1, 2, 3, 3))),
                           new FileWithPersons("other.jpg", new PicasaPersonLocation("Bob"), new PicasaPersonLocation("Alice")),
                           new FileWithPersons("file.jpg", new PicasaPersonLocation("Alice"), new PicasaPersonLocation("Bob")),
                       };

            // act
            var results = suts.Select(currentSut => currentSut.GetHashCode()).ToList();

            // assert
            results.Should().OnlyHaveUniqueItems();
        }
    }
}
