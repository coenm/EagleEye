namespace EagleEye.Core.Test.DefaultImplementations
{
    using System;

    using EagleEye.Core.DefaultImplementations;

    using FluentAssertions;

    using Xunit;

    public class SystemDateTimeServiceTest
    {
        private readonly SystemDateTimeService _sut;

        public SystemDateTimeServiceTest()
        {
            _sut = SystemDateTimeService.Instance;
        }

        [Fact]
        public void SystemDateTimeServiceNowShouldUseSystemDateTimeNowTest()
        {
            // Not able to check if sut uses System.DateTime to get the current datetime.
            // This test checks if the resulting value is almost the same as the current DateTime.Now.

            // act
            var result = _sut.Now;

            // assert
            result.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void SystemDateTimeServiceTodayShouldUseSystemDateTimeTodayTest()
        {
            // Not able to check if sut uses System.DateTime to get the current datetime.
            // This test checks if the resulting value is the same as the current DateTime.Today
            // This test might fail when ran at midnight. For now, this acceptable.

            // act
            var result = _sut.Today;

            // assert
            result.Should().Be(DateTime.Today);
        }
    }
}
