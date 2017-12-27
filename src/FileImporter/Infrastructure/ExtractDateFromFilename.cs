using System;
using System.Text.RegularExpressions;

namespace FileImporter.Infrastructure
{
    public static class ExtractDateFromFilename
    {

        public static DateTime? TryGetFromFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var f = filename.Trim().ToUpper();

            f = f.Replace('-', ' ');
            f = f.Replace('_', ' ');

            if (f.StartsWith("IMG"))
            {
                f = f.Substring(3).Trim();
            }

            if (f.StartsWith("VID"))
            {
                f = f.Substring(3).Trim();
            }

            var r = new Regex("([0-9]{4})([0-9]{2})([0-9]{2}).*");
            var match = r.Match(f);
            if (!match.Success)
                return DateTime.MinValue;

            var year = Convert.ToInt32(match.Groups[1].Value);
            var month = Convert.ToInt32(match.Groups[2].Value);
            var day = Convert.ToInt32(match.Groups[3].Value);

            if (month == 0 || month > 12)
                return null;
            if (day == 0 | day > 31)
                return null;
            
            return new DateTime(year, month, day, 0,0,0);
        }
    }
}