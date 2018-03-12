namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ExifToolAdapter : IExifTool
    {
        private readonly OpenedExifTool _exiftoolImpl;

        public ExifToolAdapter(string exiftoolExecutable)
        {
            _exiftoolImpl = new OpenedExifTool(exiftoolExecutable);
            _exiftoolImpl.Init();
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            var result = await _exiftoolImpl.ExecuteAsync(filename).ConfigureAwait(false);

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
            _exiftoolImpl
                .DisposeAsync(new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token)
                .GetAwaiter()
                .GetResult();
        }
    }
}