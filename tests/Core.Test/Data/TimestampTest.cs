namespace EagleEye.Core.Test.Data
{
    using System;

    using EagleEye.Core.Data;
    using FluentAssertions;
    using Xunit;

    public class TimestampTest
    {
        [Fact]
        public void SetYear_ValueShouldBeDatTimeAndPrecisionShouldBeYearTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2000);

            // assert
            sut.Value.Should().Be(new DateTime(2000, 1, 1));
            sut.Precision.Should().Be(TimestampPrecision.Year);
        }

        [Fact]
        public void SetInvalidYear_ThrowsExceptionTest()
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(-1);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetYearMonth_ValueShouldBeDatTimeAndPrecisionShouldBeMonthTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2001, 11);

            // assert
            sut.Value.Should().Be(new DateTime(2001, 11, 1));
            sut.Precision.Should().Be(TimestampPrecision.Month);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void SetInvalidMonth_ThrowsExceptionTest(int month)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(2017, month);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetYearMonthDay_ValueShouldBeDatTimeAndPrecisionShouldBeDayTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2011, 6, 1);

            // assert
            sut.Value.Should().Be(new DateTime(2011, 6, 1));
            sut.Precision.Should().Be(TimestampPrecision.Day);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(32)]
        public void SetInvalidDay_ThrowsExceptionTest(int day)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(2017, 1, day);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(2016, false)] // leap year
        [InlineData(2017, true)] // regular year
        public void SetLeapYearTest(int year, bool errorExpected)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(year, 2, 29);

            // assert
            if (errorExpected)
                act.Should().Throw<ArgumentOutOfRangeException>();
            else
                act.Should().NotThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetYearMonthDayHour_ValueShouldBeDatTimeAndPrecisionShouldBeHourTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2011, 6, 1, 23);

            // assert
            sut.Value.Should().Be(new DateTime(2011, 6, 1, 23, 0, 0));
            sut.Precision.Should().Be(TimestampPrecision.Hour);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(24)]
        public void SetInvalidHour_ThrowsExceptionTest(int hour)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(2017, 1, 16, hour);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetYearMonthDayHourMinute_ValueShouldBeDatTimeAndPrecisionShouldBeMinuteTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2011, 6, 1, 23, 0);

            // assert
            sut.Value.Should().Be(new DateTime(2011, 6, 1, 23, 0, 0));
            sut.Precision.Should().Be(TimestampPrecision.Minute);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(60)]
        public void SetInvalidMinute_ThrowsExceptionTest(int minute)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(2017, 1, 16, 23, minute);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SetYearMonthDayHourMinuteSecond_ValueShouldBeDatTimeAndPrecisionShouldBeMinuteTest()
        {
            // arrange

            // act
            var sut = new Timestamp(2011, 6, 1, 23, 0, 12);

            // assert
            sut.Value.Should().Be(new DateTime(2011, 6, 1, 23, 0, 12));
            sut.Precision.Should().Be(TimestampPrecision.Second);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(60)]
        public void SetInvalidSecond_ThrowsExceptionTest(int minute)
        {
            // arrange

            // act
            Action act = () => _ = new Timestamp(2017, 1, 16, 23, 12, minute);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(2018, null, null, null, null, null, "20180000000000")]
        [InlineData(2018, 5, null, null, null, null, "20180500000000")]
        [InlineData(2018, 11, 17, null, null, null, "20181117000000")]
        [InlineData(2018, 11, 22, 15, null, null, "20181122150000")]
        [InlineData(2018, 11, 22, 15, 16, null, "20181122151600")]
        [InlineData(2018, 11, 22, 15, 16, 17, "20181122151617")]
        public void ToString_ShouldRespectPrecisionTest(int year, int? month, int? day, int? hour, int? minute, int? seconds, string expectedResult)
        {
            // arrange
            var sut = new Timestamp(year, month, day, hour, minute, seconds);

            // act
            var result = sut.ToString();

            // assert
            result.Should().Be(expectedResult);
        }

        [Theory]
        [ClassData(typeof(TimestampVersusTimestampExpectation))]
        public void Equals_ShouldActAsValueObject_WhenComparedToTimestamp(Timestamp timestamp1, Timestamp timestamp2, bool expectedResult, string reason)
        {
            // arrange

            // act
            var result = timestamp1.Equals(timestamp2);

            // assert
            result.Should().Be(expectedResult, reason);
        }

        [Theory]
        [ClassData(typeof(TimestampVersusTimestampExpectation))]
        public void Equals_ShouldActAsValueObject_WhenComparedToObject(Timestamp timestamp1, Timestamp timestamp2, bool expectedResult, string reason)
        {
            // arrange

            // act
            var result = timestamp1.Equals((object)timestamp2);

            // assert
            result.Should().Be(expectedResult, reason);
        }

        [Theory]
        [InlineData(null, "null should not be equal to Timestamp.")]
        [InlineData("not a timestamp", "Other type of object should not be equal to Timestamp.")]
        public void Equals_ShouldReturnFalse_WhenComparedToSomethingElseThanTimestampObject(object timestamp2, string reason)
        {
            // arrange
            var timestamp1 = new Timestamp(2000);

            // act
            var result = timestamp1.Equals(timestamp2);

            // assert
            result.Should().BeFalse(reason);
        }

        [Theory]
        [ClassData(typeof(TimestampVersusTimestampExpectation))]
        public void GetHashCode_ShouldActAsValueObject(Timestamp timestamp1, Timestamp timestamp2, bool expectedResult, string reason)
        {
            // arrange

            // act
            var hashCode1 = timestamp1.GetHashCode();
            var hashCode2 = timestamp2.GetHashCode();

            // assert
            (hashCode1 == hashCode2).Should().Be(expectedResult, reason);
        }

        private class TimestampVersusTimestampExpectation : TheoryData<Timestamp, Timestamp, bool, string>
        {
            public TimestampVersusTimestampExpectation()
            {
                var timestampYear1999 = new Timestamp(1999);
                var timestampYear2000 = new Timestamp(2000);
                var timestampMonth200002 = new Timestamp(2000, 02);

                Add(timestampYear2000, timestampYear2000, true, "Same object should be equal (reference equal).");

                Add(timestampYear2000, new Timestamp(2000), true, "Value objects are the same (same year).");
                Add(timestampYear2000, timestampYear1999, false, "Value objects have different year.");
                Add(timestampYear2000, timestampMonth200002, false, "Value objects have same year but different month.");

                Add(timestampMonth200002, new Timestamp(2000, 02), true, "Value objects are the same (same year and month)");
            }
        }
    }
}
