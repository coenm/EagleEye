namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Medallion.Shell;

    using Nito.AsyncEx;


    public static class OpenedExifToolSimpleExtensions
    {
        public static Task<string> GetVersionAsync(this OpenedExifToolSimple @this, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { "-ver" }, ct);
        }

        public static Task<string> ExecuteAsync(this OpenedExifToolSimple @this, string singleArg, CancellationToken ct = default(CancellationToken))
        {
            return @this.ExecuteAsync(new[] { singleArg }, ct);
        }
    }

    public class OpenedExifToolSimple : IDisposable
    {
        private readonly string _exifToolPath;
        private readonly object _syncLock = new object();
        private readonly AsyncLock _syncLockAddToExifTool = new AsyncLock();

        private readonly AsyncLock _executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _executeImpAsyncSyncLock = new AsyncLock();

        private readonly List<string> _defaultArgs;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _waitingTasks;
        private ExifToolStayOpenStream _stream;
        private Command _cmd;
        private int _key;
        private bool _disposed;

        private bool _initialized;
        private bool _closed;

        private readonly CancellationTokenSource _stopQueueCts;

        public OpenedExifToolSimple(string exifToolPath)
        {
            _stopQueueCts = new CancellationTokenSource();
            _initialized = false;
            _disposed = false;
            _closed = false;
            _key = 0;
            _exifToolPath = exifToolPath;
            _defaultArgs = new List<string>
            {
                ExifToolArguments.STAY_OPEN,
                ExifToolArguments.BOOL_TRUE,
                "-@",
                "-",
                ExifToolArguments.COMMON_ARGS,
                ExifToolArguments.JSON_OUTPUT,

                // format coordinates as signed decimals.
                "-c",
                "%+.6f",

                "-struct",
                "-g", // group
            };

            _waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        }

        public void Init()
        {
            if (_initialized)
                return;

            lock (_syncLock)
            {
                if (_initialized)
                    return;

                _stream = new ExifToolStayOpenStream(Encoding.UTF8);
                _stream.Update += StreamOnUpdate;

                _cmd = Command.Run(_exifToolPath, _defaultArgs)
                    .RedirectTo(_stream);

                _initialized = true;
            }
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default(CancellationToken))
        {
            _stopQueueCts.Token.ThrowIfCancellationRequested();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _stopQueueCts.Token);

            using (await _executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                return await ExecuteImpAsync(args, ct).ConfigureAwait(false);
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                if (_disposed)
                    return;

                try
                {
                    _stopQueueCts?.Cancel();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE }.AsEnumerable();
                    var stoppingTask = ExecuteImpAsync(command, CancellationToken.None);
                }
                catch (Exception)
                {
                    // ignore for now.
                }

                _disposed = true;

                if (_stream != null)
                    _stream.Update -= StreamOnUpdate;

                if (_cmd?.Task != null)
                {
                    try
                    {
                        if (!_cmd.Task.Wait(TimeSpan.FromSeconds(10)))
                            _cmd.Kill();
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }

                try
                {
                    _stream?.Dispose();
                }
                catch (Exception)
                {
                    // ignore for now.
                }

                _stream = null;
                _cmd = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task<string> ExecuteImpAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await _executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                var tcs = new TaskCompletionSource<string>();
                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    var key = Interlocked.Increment(ref _key).ToString();

                    if (!_waitingTasks.TryAdd(key, tcs))
                        throw new Exception("Could not execute");

                    await AddToExifToolAsync(key, args).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
        }

        private void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            if (_waitingTasks.TryRemove(dataCapturedArgs.Key, out var tcs))
            {
                tcs.TrySetResult(dataCapturedArgs.Data);
            }
        }

        private async Task AddToExifToolAsync(string key, IEnumerable<string> args)
        {
            foreach (var arg in args)
                await _cmd.StandardInput.WriteLineAsync(arg).ConfigureAwait(false);

            await _cmd.StandardInput.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
        }
    }
}