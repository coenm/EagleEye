namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model
{
    using System;

    internal class Timestamp
    {
//        internal Timestamp(int year, int? month = null, int? day = null, int? hour = null, int? minutes = null, int? seconds = null)
//        {
//            if (year < 0)
//                throw new ArgumentOutOfRangeException(nameof(year), "Only after year 0.");
//
//            if (month == null)
//            {
//                Value = new DateTime(year, 1, 1);
//                Precision = TimestampPrecision.Year;
//                return;
//            }
//
//            if (month < 1 || month > 12)
//                throw new ArgumentOutOfRangeException(nameof(month));
//
//            if (day == null)
//            {
//                Value = new DateTime(year, month.Value, 1);
//                Precision = TimestampPrecision.Month;
//                return;
//            }
//
//            if (day < 1)
//                throw new ArgumentOutOfRangeException(nameof(day));
//
//            if (day > DateTime.DaysInMonth(year, month.Value))
//                throw new ArgumentOutOfRangeException(nameof(day));
//
//            if (hour == null)
//            {
//                Value = new DateTime(year, month.Value, day.Value);
//                Precision = TimestampPrecision.Day;
//                return;
//            }
//
//            if (hour < 0 || hour > 23)
//                throw new ArgumentOutOfRangeException(nameof(hour));
//
//            if (minutes == null)
//            {
//                Value = new DateTime(year, month.Value, day.Value, hour.Value, 0, 0);
//                Precision = TimestampPrecision.Hour;
//                return;
//            }
//
//            if (minutes < 0 || minutes > 59)
//                throw new ArgumentOutOfRangeException(nameof(minutes));
//
//            if (seconds == null)
//            {
//                Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, 0);
//                Precision = TimestampPrecision.Minute;
//                return;
//            }
//
//            if (seconds < 0 || seconds > 59)
//                throw new ArgumentOutOfRangeException(nameof(seconds));
//
//            Value = new DateTime(year, month.Value, day.Value, hour.Value, minutes.Value, seconds.Value);
//            Precision = TimestampPrecision.Second;
//        }

        internal Timestamp(DateTime dateTimeTaken, TimestampPrecision precision)
        {
            Value = dateTimeTaken;
            Precision = precision;
        }

        public DateTime Value { get; set; }

        public TimestampPrecision Precision { get; set; }

        public static Timestamp FromDateTime(DateTime value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return new Timestamp(value, TimestampPrecision.Second);
//            return new Timestamp(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
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
    }
}
