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
            return Precision switch
            {
                TimestampPrecision.Year => Value.ToString("yyyy0000000000"),
                TimestampPrecision.Month => Value.ToString("yyyyMM00000000"),
                TimestampPrecision.Day => Value.ToString("yyyyMMdd000000"),
                TimestampPrecision.Hour => Value.ToString("yyyyMMddHH0000"),
                TimestampPrecision.Minute => Value.ToString("yyyyMMddHHmm00"),
                TimestampPrecision.Second => Value.ToString("yyyyMMddHHmmss"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
