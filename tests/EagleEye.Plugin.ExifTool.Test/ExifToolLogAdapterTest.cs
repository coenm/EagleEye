namespace EagleEye.ExifTool.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using NLog;
    using NLog.Targets;
    using Xunit;

    public class ExifToolLogAdapterTest : IDisposable
    {
        private readonly ExifToolLogAdapter sut;
        private MemoryTarget logTarget;

        public ExifToolLogAdapterTest()
        {
            sut = new ExifToolLogAdapter();
        }

        [Theory]
        [MemberData(nameof(NLogEnabledLogLevels))]
        public void IsEnabled_ShouldReturnTrue_WhenNLogIsEnabled(LogLevel logLevel)
        {
            // arrange
            InitializeNLog(logLevel);

            // act
            var result = sut.IsEnabled();

            // assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsEnabled_ShouldReturnFalse_WhenNLogIsNotEnabled()
        {
            // arrange
            InitializeNLog(LogLevel.Off);

            // act
            var result = sut.IsEnabled();

            // assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            logTarget?.Dispose();
        }

        private void InitializeNLog(LogLevel logLevel)
        {
            logTarget = new MemoryTarget { Layout = "${message}" };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(logTarget, logLevel);
        }

        public static IEnumerable<object[]> NLogEnabledLogLevels() => LogLevel.AllLoggingLevels.Select(logLevel => new object[] { logLevel, });
    }
}
