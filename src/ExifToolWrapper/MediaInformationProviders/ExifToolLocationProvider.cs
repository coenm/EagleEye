namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    public class ExifToolLocationProvider : IMediaInformationProvider
    {
        private readonly IExifTool _exiftool;

        public ExifToolLocationProvider(IExifTool exiftool)
        {
            _exiftool = exiftool;
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

            JObject data;
            string s;

            data = result["XMP"] as JObject;
            if (data != null)
            {
                s = TryGetString(data, "CountryCode");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.CountryCode = s;

                s = TryGetString(data, "Location");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.SubLocation = s;

                s = TryGetString(data, "Country");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.CountryName = s;

                s = TryGetString(data, "State");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.State = s;

                s = TryGetString(data, "City");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.City = s;
            }

            data = result["XMP-iptcCore"] as JObject;
            if (data != null)
            {
                s = TryGetString(data, "CountryCode");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.CountryCode = s;

                s = TryGetString(data, "Location");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.SubLocation = s;
            }

            data = result["XMP-photoshop"] as JObject;
            if (data != null)
            {
                s = TryGetString(data, "Country");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.CountryName = s;

                s = TryGetString(data, "State");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.State = s;

                s = TryGetString(data, "City");
                if (!string.IsNullOrWhiteSpace(s))
                    media.Location.City = s;
            }
        }

        private string TryGetString([NotNull] JObject data, [NotNull] string key)
        {
            if (!(data[key] is JToken token))
                return null;

            if (token.Type == JTokenType.String)
                return token.Value<string>();

            return null;
        }
    }
}