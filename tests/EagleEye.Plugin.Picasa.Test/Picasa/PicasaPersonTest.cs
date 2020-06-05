namespace EagleEye.Picasa.Test.Picasa
{
    using System.Collections.Generic;

    using EagleEye.Picasa.Picasa;
    using FluentAssertions;
    using Xunit;

    public class PicasaPersonTest
    {
        [Fact]
        public void Properties_ShouldReturnInitialValues()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var name = sut.Name;
            var id = sut.Id;

            // assert
            name.Should().Be("Michael Jordan");
            id.Should().Be("123123");
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenNull()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(null);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherPersonIdValues()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(new PicasaPerson("123123x", "Michael Jordan"));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherPersonNameValues()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(new PicasaPerson("123123", "MichaelJordan"));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToSameObjectReference()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(sut);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToOtherRelativeRegionWithSameValues()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(new PicasaPerson("123123", "Michael Jordan"));

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenComparedToOtherObjectWithSameValues()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");
            object relativeRegion = (object)new PicasaPerson("123123", "Michael Jordan");

            // act
            var result = sut.Equals(relativeRegion);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenComparedToString()
        {
            // arrange
            var sut = new PicasaPerson("123123", "Michael Jordan");
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
            var sut1 = new PicasaPerson("1231231", "Michael Jordan1");
            var sut2 = new PicasaPerson("1231231", "Michael Jordan2");
            var sut3 = new PicasaPerson("1231232", "Michael Jordan1");

            // act
            var results = new List<int>(4)
                          {
                              sut1.GetHashCode(),
                              sut2.GetHashCode(),
                              sut3.GetHashCode(),
                          };

            // assert
            results.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void ToString_ShouldReturn_WhenIsHasValue()
        {
            // arrange
            var sut = new PicasaPerson("123", "Michael Jordan");

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("Michael Jordan (123)");
        }

        [Fact]
        public void ToString_ShouldReturn_WhenIdIsEmpty()
        {
            // arrange
            var sut = new PicasaPerson(string.Empty, "Michael Jordan");

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be("Michael Jordan (<empty id>)");
        }
    }
}
