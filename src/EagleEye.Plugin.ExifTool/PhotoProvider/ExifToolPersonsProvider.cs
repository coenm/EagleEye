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

            var persons = GetTagsFromFullJsonObject(exiftoolResult);
            return persons.Distinct().ToList();
        }

        private static IEnumerable<string> GetTagsFromSingleJsonObject([NotNull] JObject jsonObject, [NotNull] string tagsKey)
        {
            if (!(jsonObject[tagsKey] is JToken tagsToken))
                return Enumerable.Empty<string>();

            if (tagsToken.Type != JTokenType.Array)
                return Enumerable.Empty<string>();

            var result = new List<string>(tagsToken.Count());
            foreach (var tag in tagsToken.Values<string>())
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    result.Add(tag);
            }

            return result;
        }

        private IEnumerable<string> GetTagsFromFullJsonObject(JObject data)
        {
            var result = new List<string>();

            foreach (var header in headers)
            {
                if (!(data[header.Key] is JObject headerObject))
                    continue;

                result.AddRange(GetTagsFromSingleJsonObject(headerObject, header.Value));
            }

            return result;
        }
    }
}
