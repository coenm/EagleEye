namespace EagleEye.ExifTool.PhotoProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class ExifToolPersonsProvider : IPhotoPersonProvider
    {
        private readonly IExifToolReader exiftool;
        private readonly Dictionary<string, string> headers;

        public ExifToolPersonsProvider([NotNull] IExifToolReader exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
            headers = new Dictionary<string, string>
                          {
                              { "XMP", "PersonInImage" },
                              { "XMP-iptcExt", "PersonInImage" },
                          };
        }

        public string Name => nameof(ExifToolPersonsProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public async Task<List<string>> ProvideAsync([NotNull] string filename)
        {
            var exiftoolResult = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (exiftoolResult == null)
                return null;

            var persons1 = GetPersonsFromFullJsonObject(exiftoolResult);
            var persons2 = GetPersonsFromSpecificStructureInJsonObject(exiftoolResult);
            return persons1.Concat(persons2).Distinct().ToList();
        }

        private static IEnumerable<string> GetPersonsFromSingleJsonObject([NotNull] JObject jsonObject, [NotNull] string tagsKey)
        {
            if (!(jsonObject[tagsKey] is { } tagsToken))
                yield break;

            if (tagsToken.Type != JTokenType.Array)
                yield break;

            foreach (var tag in tagsToken.Values<string>())
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    yield return tag;
            }
        }

        private static IEnumerable<string> GetPersonsFromSpecificStructureInJsonObject([NotNull] JObject data)
        {
            if (!(data["XMP"] is JObject headerObject))
                yield break;

            if (!(headerObject["RegionInfoMP"] is JObject regionInfoObject))
                yield break;

            if (!(regionInfoObject["Regions"] is { } regionObject))
                yield break;

            if (regionObject.Type != JTokenType.Array)
                yield break;

            foreach (var tag in regionObject.Values<JObject>())
            {
                if (!(tag["PersonDisplayName"] is { } personDisplayNameToken))
                    continue;

                if (personDisplayNameToken.Type != JTokenType.String)
                    continue;

                yield return personDisplayNameToken.Value<string>();
            }
        }

        private IEnumerable<string> GetPersonsFromFullJsonObject([NotNull] JObject data)
        {
            var result = new List<string>();

            foreach (var header in headers)
            {
                if (!(data[header.Key] is JObject headerObject))
                    continue;

                result.AddRange(GetPersonsFromSingleJsonObject(headerObject, header.Value));
            }

            return result;
        }
    }
}
