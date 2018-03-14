﻿namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    public class ExifToolGpsProvider : IMediaInformationProvider
    {
        private readonly IExifTool _exiftool;
        private readonly NumberFormatInfo _numberFormat;

        public ExifToolGpsProvider(IExifTool exiftool)
        {
            _exiftool = exiftool;
            _numberFormat = new NumberFormatInfo();
        }

        public int Priority { get; } = 100;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await _exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return;

            var coordinate = GetGpsCoordinatesFromFullJsonObject(result);
            if (coordinate == null)
                return;

            media.Location.SetCoordinate(coordinate);
        }

        private Coordinate GetGpsCoordinatesFromFullJsonObject(JObject data)
        {
            string[] headers = { "EXIF", "XMP", "XMP-exif", "Composite", "GPS" };

            foreach (var header in headers)
            {
                if (!(data[header] is JObject headerObject))
                    continue;

                var result = GetGpsCoordinatesFromSingleJsonObject(headerObject, "GPSLatitude", "GPSLongitude", "GPSLatitudeRef", "GPSLongitudeRef");

                if (result == null)
                    continue;

                return result;
            }

            return null;
        }

        private Coordinate GetGpsCoordinatesFromSingleJsonObject([NotNull] JObject exif, [NotNull] string latitudeKey, [NotNull] string longitudeKey, [CanBeNull] string latitudeRefKey = null, [CanBeNull] string longitudeRefKey = null)
        {
            var latitude = GetLongitudeOrLatitude(exif, latitudeKey, latitudeRefKey);
            if (float.IsNaN(latitude))
                return null;

            var longitude = GetLongitudeOrLatitude(exif, longitudeKey, longitudeRefKey);
            if (float.IsNaN(longitude))
                return null;

            return new Coordinate(latitude, longitude);
        }

        private float GetLongitudeOrLatitude([NotNull] JObject jsonObject, [NotNull] string key, [CanBeNull]string refKey)
        {
            var result = float.NaN;

            if (!(jsonObject[key] is JToken latToken))
                return result;

            result = GetFloatFromToken(latToken);

            if (string.IsNullOrWhiteSpace(refKey))
                return result;

            if (!(jsonObject[refKey] is JToken refToken))
                return result;

            var value = refToken.Value<string>();

            if (string.IsNullOrWhiteSpace(value))
                return result;

            if (result <= 0)
                return result;

            if (string.Equals(value, "West", StringComparison.InvariantCultureIgnoreCase)
                ||
                string.Equals(value, "South", StringComparison.InvariantCultureIgnoreCase))
            {
                result *= -1;
            }

            return result;
        }

        private float GetFloatFromToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Float:
                    return token.Value<float>();

                case JTokenType.Integer:
                    return token.Value<int>();

                case JTokenType.String:
                    return TryConvertToFloat(token.Value<string>());
            }

            return float.NaN;
        }

        private float TryConvertToFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return float.NaN;

            if (float.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, _numberFormat, out var result))
                return result;

            return float.NaN;
        }
    }
}