namespace EagleEye.Picasa.Test.Picasa
{
    using System.Collections.Generic;

    using EagleEye.Picasa.Picasa;
    using FluentAssertions;
    using Xunit;

    public class RelativeRegionTest
    {
        [Fact]
        public void Equals_ShouldBeFalse_WhenNull()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);

            // act
            var result = sut.Equals(null);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherValues()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);

            // act
            var result = sut.Equals(new RelativeRegion(12, 13, 14, 16));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToSameObjectReference()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);

            // act
            var result = sut.Equals(sut);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToOtherRelativeRegionWithSameValues()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);

            // act
            var result = sut.Equals(new RelativeRegion(12, 13, 14, 15));

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenComparedToOtherObjectWithSameValues()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);
            object relativeRegion = (object)new RelativeRegion(12, 13, 14, 15);

            // act
            var result = sut.Equals(relativeRegion);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenComparedToString()
        {
            // arrange
            var sut = new RelativeRegion(12, 13, 14, 15);
            object noteRelativeRegionObject = new string('a', 100);

            // act
            var result = sut.Equals(noteRelativeRegionObject);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldDependOnAllFourProperties()
        {
            // arrange
            var sut1 = new RelativeRegion(10, 12, 13, 14);
            var sut2 = new RelativeRegion(11, 10, 13, 14);
            var sut3 = new RelativeRegion(11, 12, 10, 14);
            var sut4 = new RelativeRegion(11, 12, 13, 10);

            // act
            var results = new List<int>(4)
                          {
                              sut1.GetHashCode(),
                              sut2.GetHashCode(),
                              sut3.GetHashCode(),
                              sut4.GetHashCode(),
                          };

            // assert
            results.Should().OnlyHaveUniqueItems();
        }
    }
}
