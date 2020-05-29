namespace EagleEye.ExifTool
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Internal;
    using Newtonsoft.Json.Linq;

    internal class ExifToolCacheDecorator : IExifToolReader
    {
        private readonly IExifToolReader decoratee;
        private readonly MemoryCache cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly MemoryCacheEntryOptions cacheEntryOptions;

        public ExifToolCacheDecorator([NotNull] IExifToolReader decoratee, [NotNull] IDateTimeService dateTimeService)
        {
            Guard.Argument(decoratee, nameof(decoratee)).NotNull();
            Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
            this.decoratee = decoratee;

            cache = new MemoryCache(
                                    new MemoryCacheOptions
                                    {
                                        SizeLimit = 32,
                                        Clock = new MemoryCacheClockAdapter(dateTimeService),
                                    });

            cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSize(1)
                                .SetPriority(CacheItemPriority.Normal)
                                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        }

        public async Task<JObject> GetMetadataAsync([NotNull] string filename, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            ct.ThrowIfCancellationRequested();

            if (cache.TryGetValue(filename, out JObject result))
                return result;

            var mutex = locks.GetOrAdd(filename, _ => new SemaphoreSlim(1, 1));

            await mutex.WaitAsync(ct).ConfigureAwait(false);

            try
            {
                if (cache.TryGetValue(filename, out result))
                    return result;

                result = await decoratee.GetMetadataAsync(filename, ct).ConfigureAwait(false);
                if (result == null)
                    return null;

                cache.Set(filename, result, cacheEntryOptions);
                return result;
            }
            finally
            {
                locks.TryRemove(filename, out _);
                mutex.Release();
            }
        }

        public ValueTask DisposeAsync()
        {
            foreach ((string _, SemaphoreSlim @lock) in locks)
                @lock.Dispose();

            locks.Clear();

            cache.Dispose();
            return decoratee.DisposeAsync();
        }

        private class MemoryCacheClockAdapter : ISystemClock
        {
            private readonly IDateTimeService dateTimeService;

            public MemoryCacheClockAdapter(IDateTimeService dateTimeService)
            {
                Guard.Argument(dateTimeService, nameof(dateTimeService)).NotNull();
                this.dateTimeService = dateTimeService;
            }

            public DateTimeOffset UtcNow => dateTimeService.UtcNow;
        }
    }
}
