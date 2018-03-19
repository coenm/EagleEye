namespace EagleEye.Core
{
    using System;

    public class Timestamp
    {
        public Timestamp(int year, int? month = null, int? day = null, int? hour = null, int? minutes = null, int? seconds = null)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only after year 0.");

            if (month == null)
            {
                Value = new DateTime(year, 1, 1);
                Precision = TimestampPrecision.Year;
                return;
            }

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            if (day == null)
            {
                Value = new DateTime(year, month.Value, 1);
                Precision = TimestampPrecision.Month;
                return;
            }

            if (day < 1)
                throw new ArgumentOutOfRangeException(nameof(day));

            if (day > DateTime.DaysInMonth(year, month.Value))
                throw new ArgumentOutOfRangeException(nameof(day));

            if (hour == null)
            {
                Value = new DateTime(year, month.Value, day.Value);
                Precision = TimestampPrecision.Day;
                return;
            }

            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour));

            if (minutes == null)
            {
                Value = new DateTime(year, month.Value, day.Value, hour.Value, 0, 0);
                Precision = TimestampPrecision.Hour;
                return;
            }

            if (minutes < 0 || minutes > 59)
                throw new ArgumentOutOfRangeException(nameof(minutes));


            if (seconds == null)
            {
                Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, 0);
                Precision = TimestampPrecision.Minute;
                return;
            }

            if (seconds < 0 || seconds > 59)
                throw new ArgumentOutOfRangeException(nameof(seconds));

            Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, seconds.Value);
            Precision = TimestampPrecision.Second;
        }

        public DateTime Value { get; }

        public TimestampPrecision Precision { get; }
    }
}