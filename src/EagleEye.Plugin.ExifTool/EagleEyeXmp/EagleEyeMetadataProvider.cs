namespace EagleEye.ExifTool.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class EagleEyeMetadataProvider : IEagleEyeMetadataProvider
    {
        private readonly IExifTool exiftool;

        public EagleEyeMetadataProvider([NotNull] IExifTool exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
        }

        public string Name => nameof(EagleEyeMetadataProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public async Task<EagleEyeMetadata> ProvideAsync(string filename)
        {
            var resultExiftool = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (resultExiftool == null)
                return null;

            return GetInformationFromFullJsonObject(resultExiftool);
        }

        [CanBeNull]
        private EagleEyeMetadata GetInformationFromFullJsonObject(JObject data)
        {
            var result = new EagleEyeMetadata();

            if (!(data["XMP-CoenmEagleEye"] is JObject headerObject))
                return null;

            DateTime? dt = null;
            if ((headerObject["EagleEyeTimestamp"] is JToken token))
            {
                dt = GetDateTimeFromJToken(token);
                if (dt.HasValue)
                    result.Timestamp = dt.Value;
            }

            var guidBytes = TryGetZ85Bytes(headerObject, "EagleEyeId");
            if (guidBytes != null)
                result.Id = new Guid(guidBytes);

            var fileHashBytes = TryGetZ85Bytes(headerObject, "EagleEyeFileHash");
            if (fileHashBytes != null)
                result.FileHash = fileHashBytes;

            if (headerObject["EagleEyeRawImageHash"] is JToken rawImageHashToken)
            {
                if (rawImageHashToken.Type == JTokenType.Array)
                {
                    var rawImageHashes = new List<string>(rawImageHashToken.Count());
                    foreach (var item in rawImageHashToken.Values<string>())
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                            rawImageHashes.Add(item);
                    }

                    foreach (var z85Encoded in rawImageHashes.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        try
                        {
                            result.RawImageHash.Add(CoenM.Encoding.Z85.Decode(z85Encoded));
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                }
            }

            return result;
        }

        private byte[] TryGetZ85Bytes([NotNull] JObject data, [NotNull] string key)
        {
            var s = TryGetString(data, key);
            if (s == null)
                return null;

            try
            {
                return CoenM.Encoding.Z85.Decode(s);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string TryGetString([NotNull] JObject data, [NotNull] string key)
        {
            if (!(data[key] is JToken token))
                return null;

            if (token.Type == JTokenType.String)
                return token.Value<string>();

            return null;
        }

        [CanBeNull]
        [Pure]
        private static DateTime? GetDateTimeFromJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Date:
                    return token.Value<DateTime>();

                case JTokenType.String:
                    var dateTimeString = token.Value<string>();
                    return ParseFullDate(dateTimeString);
            }

            return null;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // Method is public for unittest purposes.
        // Improvement by extracting to new (static?) class
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
