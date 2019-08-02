namespace EagleEye.Photo.Domain.Test.CommandHandlers.Mapper
{
    using EagleEye.Photo.Domain.Commands.Inner;
    using FluentAssertions;
    using Xunit;

    using Sut = EagleEye.Photo.Domain.CommandHandlers.Mapper.TimestampPrecisionMapper;

    public class TimestampPrecisionMapperTest
    {
        [Theory]
        [InlineData(TimestampPrecision.Second, Domain.Aggregates.TimestampPrecision.Second)]
        [InlineData(TimestampPrecision.Minute, Domain.Aggregates.TimestampPrecision.Minute)]
        [InlineData(TimestampPrecision.Hour, Domain.Aggregates.TimestampPrecision.Hour)]
        [InlineData(TimestampPrecision.Day, Domain.Aggregates.TimestampPrecision.Day)]
        [InlineData(TimestampPrecision.Month, Domain.Aggregates.TimestampPrecision.Month)]
        [InlineData(TimestampPrecision.Year, Domain.Aggregates.TimestampPrecision.Year)]
        public void Convert(TimestampPrecision input, Domain.Aggregates.TimestampPrecision expectedOutput)
        {
            // arrange

            // act
            var result = Sut.Convert(input);

            // assert
            result.Should().Be(expectedOutput);
        }
    }
}
