namespace EagleEye.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using CoenM.ExifToolLib;
    using Dawn;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class ExifToolAdapter : IExifTool
    {
        private readonly AsyncExifTool exiftoolImpl;

        public ExifToolAdapter([NotNull] IExifToolConfig config, [CanBeNull] IReadOnlyCollection<string> defaultArgs)
        {
            Guard.Argument(config, nameof(config)).NotNull();

            var exiftoolConfig = new AsyncExifToolConfiguration(config.ExifToolExe, Encoding.UTF8, defaultArgs);
            exiftoolImpl = new AsyncExifTool(exiftoolConfig);
            exiftoolImpl.Initialize();
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            var result = await exiftoolImpl.ExecuteAsync(filename).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
                return null;

            try
            {
                var jsonResult = JsonConvert.DeserializeObject(result);
                var jsonArray = jsonResult as JArray;
                if (jsonArray?.Count != 1)
                    return null;

                return jsonArray[0] as JObject;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            exiftoolImpl.Dispose();
        }
    }
}
