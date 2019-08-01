namespace EagleEye.Photo.Domain.Test.Aggregates
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;
    using FluentAssertions;
    using Xunit;

    public class TimestampTest
    {
        private const int DefaultMonth = 1;
        private const int DefaultDay = 1;

        [Fact]
        public void CtorYear()
        {
            // arrange

            // act
            var sut = new Timestamp(1);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Year);
            sut.Value.Should().Be(new DateTime(1, DefaultMonth, DefaultDay));
            sut.ToString().Should().Be("00010000000000");
        }

        [Fact]
        public void CtorYearMonth()
        {
            // arrange

            // act
            var sut = new Timestamp(2, 9);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Month);
            sut.Value.Should().Be(new DateTime(2, 9, DefaultDay));
            sut.ToString().Should().Be("00020900000000");
        }

        [Fact]
        public void CtorYearMonthDay()
        {
            // arrange

            // act
            var sut = new Timestamp(2, 9, 16);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Day);
            sut.Value.Should().Be(new DateTime(2, 9, 16));
            sut.ToString().Should().Be("00020916000000");
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenSameValuesAreUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);
            var t2 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1.Equals(t2);

            // assert
            result.Should().BeTrue("sut is a value object");
        }

        [Fact]
        public void Equals_ShouldBeTrue_WhenSameObjectIsUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1.Equals(t1);

            // assert
            result.Should().BeTrue("sut is a value object");
        }
        [Fact]
        public void Equals_ShouldBeFalse_WhenEqualsNull()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);
            Timestamp t2 = null;

            // act
            // ReSharper disable once ExpressionIsAlwaysNull
            var result = t1.Equals(t2);

            // assert
            result.Should().BeFalse("sut is a value object");
        }

        [Fact]
        public void EqualsObject_ShouldBeTrue_WhenSameValuesAreUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);
            object t2 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1.Equals(t2);

            // assert
            result.Should().BeTrue("sut is a value object");
        }

        [Fact]
        public void EqualsObject_ShouldBeTrue_WhenSameObjectIsUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1.Equals((object)t1);

            // assert
            result.Should().BeTrue("sut is a value object");
        }

        [Fact]
        public void EqualsObject_ShouldBeFalse_WhenEqualsNull()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1.Equals((object)null);

            // assert
            result.Should().BeFalse("sut is a value object");
        }
    }
}
