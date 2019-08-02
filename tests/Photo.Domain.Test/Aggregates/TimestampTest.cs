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
        public void CtorYearMonthDayHour()
        {
            // arrange

            // act
            var sut = new Timestamp(2, 9, 16, 21);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Hour);
            sut.Value.Should().Be(new DateTime(2, 9, 16, 21, 0, 0));
            sut.ToString().Should().Be("00020916210000");
        }

        [Fact]
        public void CtorYearMonthDayHourMin()
        {
            // arrange

            // act
            var sut = new Timestamp(2, 9, 16, 21, 54);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Minute);
            sut.Value.Should().Be(new DateTime(2, 9, 16, 21, 54, 0));
            sut.ToString().Should().Be("00020916215400");
        }

        [Fact]
        public void CtorYearMonthDayHourMinSec()
        {
            // arrange

            // act
            var sut = new Timestamp(2, 9, 16, 21, 54, 28);

            // assert
            sut.Precision.Should().Be(TimestampPrecision.Second);
            sut.Value.Should().Be(new DateTime(2, 9, 16, 21, 54, 28));
            sut.ToString().Should().Be("00020916215428");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(0, 0)]
        [InlineData(0, 13)]
        [InlineData(0, 1, 0)]
        [InlineData(0, 1, 32)]
        [InlineData(0, 2, 29)]
        [InlineData(0, 6, 6, -1)]
        [InlineData(0, 6, 6, 24)]
        [InlineData(0, 6, 6, 12, -1)]
        [InlineData(0, 6, 6, 12, 60)]
        [InlineData(0, 6, 6, 12, 30, -1)]
        [InlineData(0, 6, 6, 12, 30, 60)]
        public void Ctor_ShouldThrowOutOfRangeException_WhenInputIsNotValid(int year, int? month = null, int? day = null, int? hour = null, int? minutes = null, int? seconds = null)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(year, month, day, hour, minutes, seconds);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
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

        [Fact]
        public void OperatorEquals_ShouldBeTrue_WhenSameValuesAreUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);
            var t2 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1 == t2;

            // assert
            result.Should().BeTrue("sut is a value object");
        }

        [Fact]
        public void OperatorEquals_ShouldBeTrue_WhenSameObjectIsUsed()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);

            // act
            #pragma warning disable 252,253
            var result = t1 == ((object)t1);
            #pragma warning restore 252,253

            // assert
            result.Should().BeTrue("sut is a value object");
        }

        [Fact]
        public void OperatorEquals_ShouldBeFalse_WhenEqualsNull()
        {
            // arrange
            var t1 = new Timestamp(2010, 11, 09, 23);

            // act
            var result = t1 == (Timestamp)null;

            // assert
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            result.Should().BeFalse("sut is a value object");
        }

    }
}
