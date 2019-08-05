namespace EagleEye.ExifTool.PhotoProvider
{
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class ExifToolLocationProvider : IPhotoLocationProvider
    {
        private readonly IExifTool exiftool;

        public ExifToolLocationProvider([NotNull] IExifTool exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
        }

        public string Name => nameof(ExifToolLocationProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public async Task<Location> ProvideAsync(string filename)
        {
            var result = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return null;

            string s;

            var location = new Location();

            if (result["XMP"] is JObject data)
            {
                s = TryGetString(data, "CountryCode");
                if (!string.IsNullOrWhiteSpace(s))
                    location.CountryCode = s;

                s = TryGetString(data, "Location");
                if (!string.IsNullOrWhiteSpace(s))
                    location.SubLocation = s;

                s = TryGetString(data, "Country");
                if (!string.IsNullOrWhiteSpace(s))
                    location.CountryName = s;

                s = TryGetString(data, "State");
                if (!string.IsNullOrWhiteSpace(s))
                    location.State = s;

                s = TryGetString(data, "City");
                if (!string.IsNullOrWhiteSpace(s))
                    location.City = s;
            }

            data = result["XMP-iptcCore"] as JObject;
            if (data != null)
            {
                s = TryGetString(data, "CountryCode");
                if (!string.IsNullOrWhiteSpace(s))
                    location.CountryCode = s;

                s = TryGetString(data, "Location");
                if (!string.IsNullOrWhiteSpace(s))
                    location.SubLocation = s;
            }

            data = result["XMP-photoshop"] as JObject;
            if (data != null)
            {
                s = TryGetString(data, "Country");
                if (!string.IsNullOrWhiteSpace(s))
                    location.CountryName = s;

                s = TryGetString(data, "State");
                if (!string.IsNullOrWhiteSpace(s))
                    location.State = s;

                s = TryGetString(data, "City");
                if (!string.IsNullOrWhiteSpace(s))
                    location.City = s;
            }

            return location;
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
