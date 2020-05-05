namespace EagleEye.ExifTool.PhotoProvider
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.ExifTool.Parsing;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class ExifToolDateTakenProvider : IPhotoDateTimeTakenProvider
    {
        private static readonly List<MetadataHeaderKeyPair> Keys = new List<MetadataHeaderKeyPair>
        {
            new MetadataHeaderKeyPair(MetadataHeaderKeyPair.Keys.Exif, MetadataHeaderKeyPair.Keys.ExifIfd, "DateTimeOriginal"),
            new MetadataHeaderKeyPair(MetadataHeaderKeyPair.Keys.Xmp, MetadataHeaderKeyPair.Keys.XmpExif, "DateTimeOriginal"),
            new MetadataHeaderKeyPair(MetadataHeaderKeyPair.Keys.Composite, MetadataHeaderKeyPair.Keys.Composite, "SubSecDateTimeOriginal"),
            new MetadataHeaderKeyPair(MetadataHeaderKeyPair.Keys.Xmp, MetadataHeaderKeyPair.Keys.XmpExif, "DateTimeDigitized"),
            new MetadataHeaderKeyPair(MetadataHeaderKeyPair.Keys.Composite, MetadataHeaderKeyPair.Keys.Composite, "SubSecCreateDate"),
        };

        private readonly IExifTool exiftool;

        public ExifToolDateTakenProvider([NotNull] IExifTool exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
        }

        public string Name => nameof(ExifToolDateTakenProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public async Task<Timestamp> ProvideAsync(string filename)
        {
            var data = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (data == null)
                return null;

            var dateTimeTaken = GetDateTimeFromFullJsonObject(data);

            return dateTimeTaken;
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return;

            var dateTimeTaken = GetDateTimeFromFullJsonObject(result);

            if (dateTimeTaken != null)
                media.SetDateTimeTaken(dateTimeTaken);
        }

        [CanBeNull]
        [Pure]
        private static Timestamp GetDateTimeFromFullJsonObject(JObject data)
        {
            foreach (var header in Keys)
            {
                var result = GetDateTimeFromKeyKeyPair(data, header.Header1, header.Key);
                if (result != null)
                    return result;

                result = GetDateTimeFromKeyKeyPair(data, header.Header2, header.Key);
                if (result != null)
                    return result;
            }

            return null;
        }

        [CanBeNull]
        [Pure]
        private static Timestamp GetDateTimeFromKeyKeyPair([NotNull] JObject data, [NotNull] string header, [NotNull] string key)
        {
            if (!(data[header] is JObject headerObject))
                return null;

            if (!(headerObject[key] is JToken token))
                return null;

            return GetDateTimeFromJToken(token);
        }

        [CanBeNull]
        [Pure]
        private static Timestamp GetDateTimeFromJToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Date:
                    var datetime = token.Value<DateTime>();
                    return Timestamp.FromDateTime(datetime);

                case JTokenType.String:
                    var dateTimeString = token.Value<string>();
                    var dt = DateTimeParsing.ParseFullDate(dateTimeString);
                    if (dt == null)
                        return null;
                    return Timestamp.FromDateTime(dt.Value);
            }

            return null;
        }
    }
}
