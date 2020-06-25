namespace EagleEye.Picasa.Test.Picasa
{
    using System.Linq;

    using EagleEye.Picasa.Picasa;
    using FluentAssertions;
    using Xunit;

    public class Rect64RelativeRegionTest
    {
        private const string Rect1 = "rect64(935f5217a1696893)";
        private const string Rect2 = "rect64(4f5c884659bb98b2)";
        private const string Rect3 = "rect64(49c9348362765a56)";
        private const string Rect4 = "rect64(66273ab58849596a)";
        private const string Rect5 = "rect64(3d4b58c75a5681b9)";

        [Theory]
        [InlineData("rect64(ffffffff)")]
        [InlineData("rect64(30004000)")]
        [InlineData("rect64(bfff3000ffff)")]
        [InlineData("rect64(cfff0000ffff4000)")]
        [InlineData("rect64(cfffbfffffffffff)")]
        [InlineData("rect64(d5b35a9)")]
        public void Ctor_ShouldNotThrow_WhenInputIsValid(string input)
        {
            // arrange
            var sut = new Rect64RelativeRegion(input);

            // act
            var rect = sut.Rect64;

            // assert
            rect.Should().Be(input);
        }

        [Fact]
        public void Properties_ShouldReturnInitialValues()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);

            // act
            var rect = sut.Rect64;
            var left = sut.Left;
            var top = sut.Top;
            var right = sut.Right;
            var bottom = sut.Bottom;

            // assert
            rect.Should().Be(Rect1);
            left.Should().Be(0.5756771F);
            top.Should().Be(0.32066834F);
            right.Should().Be(0.630518F);
            bottom.Should().Be(0.40849927F);
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenNull()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);

            // act
            var result = sut.Equals(null);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenOtherValues()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);

            // act
            var result = sut.Equals(new Rect64RelativeRegion(Rect2));

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToSameObjectReference()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);

            // act
            var result = sut.Equals(sut);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenCompareToOtherRelativeRegionWithSameValues()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);

            // act
            var result = sut.Equals(new Rect64RelativeRegion(Rect1));

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenComparedToOtherObjectWithSameValues()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);
            object relativeRegion = (object)new Rect64RelativeRegion(Rect1);

            // act
            var result = sut.Equals(relativeRegion);

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldBeFalse_WhenComparedToString()
        {
            // arrange
            var sut = new Rect64RelativeRegion(Rect1);
            object noteRelativeRegionObject = new string('a', 100);

            // act
            var result = sut.Equals(noteRelativeRegionObject);

            // assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetHashCode_ShouldBeDifferentWheRectIsDifferent()
        {
            // arrange
            var suts = new[]
                       {
                           new Rect64RelativeRegion(Rect1),
                           new Rect64RelativeRegion(Rect2),
                           new Rect64RelativeRegion(Rect3),
                           new Rect64RelativeRegion(Rect4),
                           new Rect64RelativeRegion(Rect5),
                       };

            // act
            var results = suts.Select(s => s.GetHashCode());

            // assert
            results.Should().OnlyHaveUniqueItems();
        }
    }
}
