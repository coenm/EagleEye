namespace EagleEye.Photo.Domain.CommandHandlers.Mapper
{
    using System;

    using EagleEye.Photo.Domain.Aggregates;

    public static class TimestampPrecisionMapper
    {
        public static TimestampPrecision Convert(Commands.Inner.TimestampPrecision precision)
        {
            switch (precision)
            {
            case Commands.Inner.TimestampPrecision.Year:
                return TimestampPrecision.Year;
            case Commands.Inner.TimestampPrecision.Month:
                return TimestampPrecision.Month;
            case Commands.Inner.TimestampPrecision.Day:
                return TimestampPrecision.Day;
            case Commands.Inner.TimestampPrecision.Hour:
                return TimestampPrecision.Hour;
            case Commands.Inner.TimestampPrecision.Minute:
                return TimestampPrecision.Minute;
            case Commands.Inner.TimestampPrecision.Second:
                return TimestampPrecision.Second;
            default:
                throw new ArgumentOutOfRangeException(nameof(precision), precision, null);
            }
        }
    }
}
