namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using Newtonsoft.Json;

    public class ExifToolAdapter : IExifTool
    {
        private readonly OpenedExifTool _exiftoolImpl;
        private readonly IEnumerable<string> _args;

        public ExifToolAdapter()
        {
            _exiftoolImpl = new OpenedExifTool("exiftool.exe");
            _exiftoolImpl.Init();
            _args = new List<string>();
        }

        public async Task<dynamic> GetMetadataAsync(string filename)
        {
            var result = await _exiftoolImpl.ExecuteAsync(filename, _args).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
                return null;

            try
            {
                var jsonResult = (dynamic)JsonConvert.DeserializeObject(result);

                var count = jsonResult.Count;
                if (count != null && (int)count == 1)
                    return jsonResult[0];
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _exiftoolImpl?.Dispose();
        }
    }
}