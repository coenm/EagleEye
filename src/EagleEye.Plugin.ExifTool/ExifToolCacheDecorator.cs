namespace EagleEye.ExifTool
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Core.Interfaces.Core;
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;
    using Newtonsoft.Json.Linq;
    using Nito.AsyncEx;

    internal class ExifToolCacheDecorator : IExifTool
    {
        private readonly AsyncLock syncLock = new AsyncLock();
        private readonly IExifTool exiftool;
        private readonly IDateTimeService dateTimeService;
        private readonly TimeSpan cacheValidity;
        private DateTime cacheTimestamp;
        private Task<JObject> task;
        private string cachedFilename;

        public ExifToolCacheDecorator([NotNull] IExifTool exiftool, [NotNull] IDateTimeService dateTimeService)
        {
            Dawn.Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            Dawn.Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            cacheValidity = TimeSpan.FromMinutes(5); // todo make this configurable.
            this.exiftool = exiftool;
            this.dateTimeService = dateTimeService;

            cachedFilename = null;
            task = null;
            cacheTimestamp = DateTime.MinValue;
        }

        public async Task<JObject> GetMetadataAsync(string filename)
        {
            Task<JObject> currentTask;

            using (await syncLock.LockAsync().ConfigureAwait(false))
            {
                var now = dateTimeService.Now;
                if (cachedFilename == null || cachedFilename != filename || now - cacheTimestamp > cacheValidity)
                {
                    cachedFilename = filename;
                    cacheTimestamp = now;
                    task = exiftool.GetMetadataAsync(filename);
                }

                currentTask = task;
            }

            // todo what if the result is an exception, should we cache that one too?
            return await currentTask.ConfigureAwait(false);
        }

        public void Dispose()
        {
            exiftool.Dispose();
        }
    }
}
