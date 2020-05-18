namespace EagleEye.Photo.Domain.CommandHandlers.Mapper
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;

    internal static class TimestampPrecisionMapper
    {
        public static TimestampPrecision Convert(Commands.Inner.TimestampPrecision precision)
        {
            return precision switch
            {
                Commands.Inner.TimestampPrecision.Year => TimestampPrecision.Year,
                Commands.Inner.TimestampPrecision.Month => TimestampPrecision.Month,
                Commands.Inner.TimestampPrecision.Day => TimestampPrecision.Day,
                Commands.Inner.TimestampPrecision.Hour => TimestampPrecision.Hour,
                Commands.Inner.TimestampPrecision.Minute => TimestampPrecision.Minute,
                Commands.Inner.TimestampPrecision.Second => TimestampPrecision.Second,
                _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, null)
            };
        }
    }
}
