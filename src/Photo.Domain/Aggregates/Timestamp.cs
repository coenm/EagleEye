namespace EagleEye.Photo.Domain.Aggregates
{
    using System;

    public class Timestamp : IEquatable<Timestamp>
    {
        public DateTime Value { get; set; }

        public TimestampPrecision Precision { get; set; }

        public static bool operator ==(Timestamp x, Timestamp y)
        {
            return x?.Equals(y) ?? y is null;
        }

        public static bool operator !=(Timestamp x, Timestamp y)
        {
            return !(x == y);
        }

        public static Timestamp Create(int year, int? month = null, int? day = null, int? hour = null, int? minutes = null, int? seconds = null)
        {
            if (year < 0)
                throw new ArgumentOutOfRangeException(nameof(year), "Only after year 0.");

            var result = new Timestamp();

            if (month == null)
            {
                if (year == 0)
                    throw new ArgumentOutOfRangeException(nameof(year), "Can only be 0 when month is given.");

                result.Value = new DateTime(year, 1, 1);
                result.Precision = TimestampPrecision.Year;
                return result;
            }

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month));

            if (day == null)
            {
                result.Value = new DateTime(year, month.Value, 1);
                result.Precision = TimestampPrecision.Month;
                return result;
            }

            if (day < 1)
                throw new ArgumentOutOfRangeException(nameof(day));

            if (day > DateTime.DaysInMonth(year, month.Value))
                throw new ArgumentOutOfRangeException(nameof(day));

            if (hour == null)
            {
                result.Value = new DateTime(year, month.Value, day.Value);
                result.Precision = TimestampPrecision.Day;
                return result;
            }

            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour));

            if (minutes == null)
            {
                result.Value = new DateTime(year, month.Value, day.Value, hour.Value, 0, 0);
                result.Precision = TimestampPrecision.Hour;
                return result;
            }

            if (minutes < 0 || minutes > 59)
                throw new ArgumentOutOfRangeException(nameof(minutes));

            if (seconds == null)
            {
                result.Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, 0);
                result.Precision = TimestampPrecision.Minute;
                return result;
            }

            if (seconds < 0 || seconds > 59)
                throw new ArgumentOutOfRangeException(nameof(seconds));

            result.Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, seconds.Value);
            result.Precision = TimestampPrecision.Second;
            return result;
        }

        public static Timestamp FromDateTime(DateTime value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return Create(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
        }

        public override string ToString()
        {
            switch (Precision)
            {
                case TimestampPrecision.Year:
                    return Value.ToString("yyyy0000000000");
                case TimestampPrecision.Month:
                    return Value.ToString("yyyyMM00000000");
                case TimestampPrecision.Day:
                    return Value.ToString("yyyyMMdd000000");
                case TimestampPrecision.Hour:
                    return Value.ToString("yyyyMMddHH0000");
                case TimestampPrecision.Minute:
                    return Value.ToString("yyyyMMddHHmm00");
                case TimestampPrecision.Second:
                    return Value.ToString("yyyyMMddHHmmss");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Equals(Timestamp other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Value.Equals(other.Value)
                   &&
                   Precision == other.Precision;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;

            return Equals((Timestamp)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ (int)Precision;
            }
        }
    }
}
