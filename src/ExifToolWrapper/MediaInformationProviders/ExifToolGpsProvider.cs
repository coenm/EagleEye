namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
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

            if (result["EXIF"] is JObject exif)
                TryUseExif(exif, media, "GPSLatitude", "GPSLongitude");

            if (result["XMP"] is JObject xmp)
                TryUseExif(xmp, media, "GPSLatitude", "GPSLongitude");

            if (result["Composite"] is JObject composite)
                TryUseExif(composite, media, "GPSLatitude", "GPSLongitude");

            // ""GPSLatitude"": "" + 40.736072"",
            // ""GPSLongitude"": -73.994293,
            if (result["XMP-exif"] is JObject xmpExif)
                TryUseExif(xmpExif, media, "GPSLatitude", "GPSLongitude");

            // "GPSLatitudeRef": "North",
            // "GPSLatitude": 40.736072,
            // "GPSLongitudeRef": "West",
            // "GPSLongitude": 73.994293,
            if (result["GPS"] is JObject gps)
                TryUseExif2(gps, media, "GPSLatitude", "GPSLongitude", "GPSLatitudeRef", "GPSLongitudeRef");
        }

        private void TryUseExif2([NotNull] JObject exif, [NotNull] MediaObject media, [NotNull] string latitudeKey, [NotNull] string longitudeKey, string latitudeRefKey, string longitudeRefKey)
        {
            var latitude = float.NaN;
            var longitude = float.NaN;

            if (exif[latitudeKey] is JToken latToken)
                latitude = GetFloatFromToken(latToken);

            if (exif[longitudeKey] is JToken lonToken)
                longitude = GetFloatFromToken(lonToken);

            if (float.IsNaN(latitude))
                return;

            if (float.IsNaN(longitude))
                return;


            if (exif[latitudeRefKey] is JToken latRefToken)
            {
                var value = latRefToken.Value<string>();
                if (value == "South")
                {
                    if (latitude > 0)
                        latitude *= -1;
                }
            }

            if (exif[longitudeRefKey] is JToken lonRefToken)
            {
                var value = lonRefToken.Value<string>();
                if (value == "West")
                {
                    if (longitude > 0)
                        longitude *= -1;
                }
            }

            media.Location.SetCoordinates(latitude, longitude);
        }

        private void TryUseExif([NotNull] JObject exif, [NotNull] MediaObject media, [NotNull]string latitudeKey, [NotNull]string longitudeKey)
        {
            var latitude = float.NaN;
            var longitude = float.NaN;

            if (exif[latitudeKey] is JToken latToken)
                latitude = GetFloatFromToken(latToken);

            if (exif[longitudeKey] is JToken lonToken)
                longitude = GetFloatFromToken(lonToken);

            if (float.IsNaN(latitude))
                return;

            if (float.IsNaN(longitude))
                return;

            media.Location.SetCoordinates(latitude, longitude);
        }

        private float GetFloatFromToken(JToken token)
        {
            if (token.Type == JTokenType.Float)
                return token.Value<float>();

            if (token.Type == JTokenType.String)
                return TryConvertToFloat(token.Value<string>());

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