namespace EagleEye.DirectoryStructure.PhotoProvider
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    [UsedImplicitly]
    internal class DirectoryStructureDateTimeProvider : IPhotoDateTimeTakenProvider
    {
        [NotNull] private readonly Regex findDateRegex;
        [NotNull] private readonly IFormatProvider numberFormatInfo;

        public DirectoryStructureDateTimeProvider()
        {
            numberFormatInfo = new NumberFormatInfo();

            findDateRegex = new Regex(
                @"^(?<year>19[\d]{2}|20[\d]{2})((?<seperator>[\. -_])(?<month>0[1-9]{1}|1[012]|[1-9])(\k<seperator>(?<day>[12][\d]|3[01]|0[1-9]|[1-9]))?)?[^\d].*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public string Name => nameof(DirectoryStructureDateTimeProvider);

        public uint Priority { get; } = 10;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public Task<Timestamp> ProvideAsync(string filename, Timestamp previousResult)
        {
            DebugHelpers.Guards.Guard.IsTrue(CanProvideInformation(filename), nameof(CanProvideInformation), "Cannot provide information.");

            filename = filename.Trim();

            try
            {
                filename = Path.GetFileName(filename);
            }
            catch (Exception)
            {
                return Task.FromResult<Timestamp>(null);
            }

            var result = findDateRegex.Match(filename);
            if (!result.Success)
                return Task.FromResult<Timestamp>(null);

            var year = result.Groups["year"];
            if (!year.Success || !TryParseInt(year.Value, out var yearValue))
                return Task.FromResult<Timestamp>(null);

            var month = result.Groups["month"];
            if (!month.Success || !TryParseInt(month.Value, out var monthValue))
                return Task.FromResult(new Timestamp(yearValue));

            var day = result.Groups["day"];
            if (!day.Success || !TryParseInt(day.Value, out var dayValue))
                return Task.FromResult(new Timestamp(yearValue, monthValue));

            try
            {
                // The regular expression assumes all months have 31 days.
                // Therefore, it might happen that an invalid date has been found.
                var ts = new Timestamp(yearValue, monthValue, dayValue);
                return Task.FromResult(ts);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Task.FromResult(new Timestamp(yearValue, monthValue));
            }
        }

        private bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.None, numberFormatInfo, out result);
        }
    }
}
