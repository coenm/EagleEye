namespace EagleEye.ExifTool.PhotoProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class ExifToolTagsProvider : IPhotoTagProvider
    {
        private readonly IExifTool exiftool;
        private readonly Dictionary<string, string> headers;

        public ExifToolTagsProvider([NotNull] IExifTool exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
            headers = new Dictionary<string, string>
                          {
                              { "XMP", "Subject" },
                              { "XMP-dc", "Subject" },
                              { "IPTC", "Keywords" },
                          };
        }

        public string Name => nameof(ExifToolTagsProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);

        public async Task<List<string>> ProvideAsync(string filename)
        {
            var resultExiftool = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (resultExiftool == null)
                return null;

            var tags = GetTagsFromFullJsonObject(resultExiftool);

            return tags?.Distinct().ToList();
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

        [CanBeNull]
        private List<string> GetTagsFromFullJsonObject(JObject data)
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
