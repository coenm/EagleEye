namespace EagleEye.ExifTool.PhotoProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using Dawn;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;

    internal class ExifToolPersonsProvider : IPhotoPersonProvider
    {
        private readonly IExifTool exiftool;
        private readonly Dictionary<string, string> headers;

        public ExifToolPersonsProvider([NotNull] IExifTool exiftool)
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

        public async Task<List<string>> ProvideAsync([NotNull] string filename, [CanBeNull] List<string> previousResult)
        {
            var exiftoolResult = await exiftool.GetMetadataAsync(filename).ConfigureAwait(false);

            if (exiftoolResult == null)
                return previousResult;

            var persons = GetTagsFromFullJsonObject(exiftoolResult);
            if (previousResult == null)
                return persons.ToList();

            previousResult.AddRange(persons);
            return previousResult;
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
