namespace EagleEye.ExifToolWrapper
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    using Nito.AsyncEx;

    public class ExifToolCacheDecorator : IExifTool
    {
        private readonly AsyncLock _syncLock = new AsyncLock();
        private readonly IExifTool _exiftool;
        private readonly IDateTimeService _dateTimeService;
        private readonly TimeSpan _cacheValidity;
        private DateTime _cacheTimestamp;
        private Task<JObject> _task;
        private string _cachedFilename;

        public ExifToolCacheDecorator([NotNull] IExifTool exiftool, [NotNull] IDateTimeService dateTimeService)
        {
            _cacheValidity = TimeSpan.FromMinutes(5); //todo make this configurable.
            _exiftool = exiftool;
            _dateTimeService = dateTimeService;

            _cachedFilename = null;
            _task = null;
            _cacheTimestamp = DateTime.MinValue;
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            Task<JObject> currentTask;

            using (await _syncLock.LockAsync().ConfigureAwait(false))
            {
                var now = _dateTimeService.Now;
                if (_cachedFilename == null || _cachedFilename != filename || now - _cacheTimestamp > _cacheValidity)
                {
                    _cachedFilename = filename;
                    _cacheTimestamp = now;
                    _task = _exiftool.GetMetadataAsync(filename);
                }

                currentTask = _task;
            }

            //todo what if the result is an exception, should we cache that one too?

            return await currentTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            _exiftool.Dispose();
        }
    }
}