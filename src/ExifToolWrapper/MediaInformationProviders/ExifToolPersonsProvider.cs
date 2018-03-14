namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Interfaces;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    public class ExifToolPersonsProvider : IMediaInformationProvider
    {
        private readonly IExifTool _exiftool;
        private readonly Dictionary<string, string> _headers;

        public ExifToolPersonsProvider(IExifTool exiftool)
        {
            _exiftool = exiftool;
            _headers = new Dictionary<string, string>
                          {
                              { "XMP", "PersonInImage" },
                              { "XMP-iptcExt", "PersonInImage" }
                          };
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

            var persons = GetTagsFromFullJsonObject(result);

            media.AddPersons(persons);
        }

        private static IEnumerable<string> GetTagsFromSingleJsonObjecty([NotNull] JObject jsonObject, [NotNull] string tagsKey)
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

            foreach (var header in _headers)
            {
                if (!(data[header.Key] is JObject headerObject))
                    continue;

                result.AddRange(GetTagsFromSingleJsonObjecty(headerObject, header.Value));
            }

            return result;
        }
    }
}