namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using Nito.AsyncEx;

    // temp interface to get things clear
    public interface IOpenedExifToolSimple
    {
        // Initialize and start exiftool
        void Init();

        Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default(CancellationToken));

        Task DisposeAsync(CancellationToken ct = default(CancellationToken));
    }

    public class OpenedExifToolSimple : IOpenedExifToolSimple
    {
        private readonly string _exifToolPath;
        private readonly AsyncLock _executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _executeImpAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _disposingSyncLock = new AsyncLock();

        private readonly List<string> _defaultArgs;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _waitingTasks;
        private readonly ExifToolStayOpenStream _stream;
        private IMedallionShell _cmd;
        private int _key;
        private bool _disposed;

        private bool _initialized;

        private readonly CancellationTokenSource _stopQueueCts;

        public OpenedExifToolSimple(string exifToolPath)
        {
            _stream = new ExifToolStayOpenStream(Encoding.UTF8);

            _stopQueueCts = new CancellationTokenSource();
            _initialized = false;
            _disposed = false;
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

            _stream.Update += StreamOnUpdate;

            CreateExitToolMedallionShell(_exifToolPath, _defaultArgs, _stream, null);

            _initialized = true;
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

        public async Task DisposeAsync(CancellationToken ct = default(CancellationToken))
        {
            if (_disposed)
                return;

            using (await _disposingSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                if (_disposed)
                    return;

                try
                {
                    // This is really not okay. Not sure why or when the stay-open False command doesn't seem to work.
                    // This is just a stupid 'workaround' and is okay for now.
                    await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);

                    var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE };
                    await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                    try
                    {
                        await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);

                        await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);

                        await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception occurred when executing stay_open false. Msg: {e.Message}");

                    Ignore(() => _cmd?.Kill());

                    _stream.Update -= StreamOnUpdate;

                    Ignore(() => _stream.Dispose());
                    _cmd = null;

                    return;
                }

                // else try to dispose grasefully
                var c = _cmd;
                if (c?.Task != null)
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        c.Kill();
                        await c.Task.ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        sw.Stop();
                        Console.WriteLine($"Exception occurred after {sw.Elapsed} when awaiting ExifTool task. Msg: {e.Message}");
                        Ignore(() => c.Kill());
                    }
                }

                _stream.Update -= StreamOnUpdate;
                Ignore(() => _stream.Dispose());
                _cmd = null;
                _disposed = true;
            }
        }

        protected virtual void CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
        {
            _cmd = new MedallionShellAdapter(exifToolPath, defaultArgs, outputStream, errorStream);
        }

        private static void Ignore(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception)
            {
                // ignore
            }
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

        private async Task ExecuteOnlyAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await _executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                await AddToExifToolAsync(null, args).ConfigureAwait(false);
            }
        }

        private async Task AddToExifToolAsync(string key, IEnumerable<string> args)
        {
            foreach (var arg in args)
                await _cmd.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(key))
                await _cmd.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
        }
    }
}