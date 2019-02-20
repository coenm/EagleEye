namespace EagleEye.ExifToolWrapper.MediaInformationProviders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core;
    using EagleEye.Core.Data;
    using EagleEye.Core.Interfaces;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;

    using Helpers.Guards;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    public class ExifToolPersonsProvider : IPhotoPersonProvider, IMediaInformationProvider
    {
        private readonly IExifTool exiftool;
        private readonly Dictionary<string, string> headers;

        public ExifToolPersonsProvider([NotNull] IExifTool exiftool)
        {
            Guard.NotNull(exiftool, nameof(exiftool));
            this.exiftool = exiftool;
            headers = new Dictionary<string, string>
                          {
                              { "XMP", "PersonInImage" },
                              { "XMP-iptcExt", "PersonInImage" },
                          };
        }

        public string Name => nameof(ExifToolPersonsProvider);

        public uint Priority { get; } = 100;

        public bool CanProvideInformation(string filename)
        {
            return true;
        }

        public async Task<List<string>> ProvideAsync(string filename)
        {
            var result = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (result == null)
                return new List<string>(0);

            var persons = GetTagsFromFullJsonObject(result);

            return persons.ToList();
        }

        public async Task ProvideAsync(string filename, MediaObject media)
        {
            var result = await ProvideAsync(filename).ConfigureAwait(false);

            media.AddPersons(result);
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
