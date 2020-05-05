namespace EagleEye.ExifTool.Parsing
{
    using System;
    using System.Globalization;

    using JetBrains.Annotations;

    internal static class DateTimeParsing
    {
        [Pure]
        internal static DateTime? ParseFullDate(string data)
        {
            if (DateTimeOffset.TryParseExact(data, "yyyy:MM:dd HH:mm:ss", null, DateTimeStyles.None, out var dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy:MM:dd HH:mm:sszzz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy-MM-dd HH:mm:sszzz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy:MM:dd HH:mm:sszz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy-MM-dd HH:mm:sszz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy:MM:dd HH:mm:ssz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            if (DateTimeOffset.TryParseExact(data, "yyyy-MM-dd HH:mm:ssz", null, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset.DateTime;

            return null;
        }
    }
}
