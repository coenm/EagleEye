namespace EagleEye.ExifToolWrapper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

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
            var result = await _exiftoolImpl.Execute(filename, _args).ConfigureAwait(false);

            // todo parse string as json and return dynamic object.
            return null;
        }

        public void Dispose()
        {
            _exiftoolImpl?.Dispose();
        }
    }
}