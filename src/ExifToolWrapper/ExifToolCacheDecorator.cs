namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    using Newtonsoft.Json.Linq;

    using Nito.AsyncEx;

    public class ExifToolCacheDecorator : IExifTool
    {
        private readonly AsyncLock _syncLock = new AsyncLock();
        private readonly IExifTool _exiftool;
        private readonly IDateTimeService _dateTimeService;
        private DateTime _cacheTimestamp;
        private Task<JObject> _task;
        private string _cachedFilename;

        public ExifToolCacheDecorator(IExifTool exiftool, IDateTimeService dateTimeService)
        {
            _exiftool = exiftool;
            _dateTimeService = dateTimeService;

            _cachedFilename = null;
            _task = null;
            _cacheTimestamp = DateTime.MinValue;
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            Task<JObject> tmp;

            using (await _syncLock.LockAsync().ConfigureAwait(false))
            {
                tmp = _task;

                if (_cachedFilename == null || _cachedFilename != filename)
                {
                    _cachedFilename = filename;
                    _cacheTimestamp = _dateTimeService.Now;
                    _task = _exiftool.GetMetadataAsync(filename);
                }
            }

            return await tmp.ConfigureAwait(false);
        }

        public void Dispose()
        {
            _exiftool?.Dispose();
        }
    }
}