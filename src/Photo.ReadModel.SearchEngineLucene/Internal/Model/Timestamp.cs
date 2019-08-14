namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model
{
    using System;

    internal class Timestamp
    {
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

            // return new Timestamp(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
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
