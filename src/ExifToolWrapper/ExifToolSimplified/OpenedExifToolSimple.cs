namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using Nito.AsyncEx;

    public class OpenedExifToolSimple : IDisposable
    {
        private readonly string _exifToolPath;
        private readonly object _syncLock = new object();
        private readonly AsyncLock _syncLockAddToExifTool = new AsyncLock();

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
        private bool _closed;

        private readonly CancellationTokenSource _stopQueueCts;

        public OpenedExifToolSimple(string exifToolPath)
        {
            _stream = new ExifToolStayOpenStream(Encoding.UTF8);

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

            _stream.Update += StreamOnUpdate;

            CreateExitToolMedallionShell(_exifToolPath, _defaultArgs, _stream, new MemoryStream());

            _initialized = true;
        }

        protected virtual void CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
        {
            _cmd = new MedallionShellAdapter(exifToolPath, defaultArgs, outputStream, errorStream);
        }

        public async Task DisposeAsync(CancellationToken ct = default(CancellationToken))
        {
            if (_disposed)
                return;

            using (await _disposingSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                if (_disposed)
                    return;
                await Task.Delay(100).ConfigureAwait(false);

                try
                {
                    var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE }.AsEnumerable();
                    await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                    try
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                        await Task.Delay(10).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                        await Task.Delay(10).ConfigureAwait(false);
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        // ignore.
                    }

                }
                catch (Exception e)
                {
                    Ignore(() => _cmd?.Kill());

                    _stream.Update -= StreamOnUpdate;

                    Ignore(() => _stream.Dispose());
                    _cmd = null;

                    return;
                }

                // else try to dispose grasefully
                var c = ((MedallionShellAdapter)_cmd);
                var cc = c.Command;
                if (c?.Task != null)
                {
                    try
                    {
                        //                    await _cmd.Task.ConfigureAwait(false);
                        c.KillAfter(TimeSpan.FromSeconds(5));
                        await c.Task.ConfigureAwait(false);

                        //                    await c.Task.ConfigureAwait(false);
                        //                    await c.Task.WaitAsync(ct).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Ignore(() => c.Kill());
                    }
                }

                _stream.Update -= StreamOnUpdate;
                Ignore(() => _stream.Dispose());
                _cmd = null;
                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            using (_disposingSyncLock.Lock())
            {
                if (_disposed)
                    return;
                try
                {
                    Thread.Sleep(100);
                    var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE }.AsEnumerable();
                    foreach (var arg in command)
                    {
                        Thread.Sleep(1);
                        _cmd.WriteLine(arg);
                    }
                }
                catch (Exception e)
                {
                    Ignore(() => _cmd?.Kill());

                    _stream.Update -= StreamOnUpdate;

                    Ignore(() => _stream.Dispose());
                    _cmd = null;

                    return;
                }


                // else try to dispose grasefully
                var c = ((MedallionShellAdapter)_cmd);
                var cc = c.Command;
                try
                {
                    cc.Task.Wait(5000);
                    //                c.KillAfter(TimeSpan.FromSeconds(5));
                    //                cc.Wait();
                }
                catch (Exception e)
                {
                    Ignore(() => c.Kill());
                }

                _stream.Update -= StreamOnUpdate;
                Ignore(() => _stream.Dispose());
                _cmd = null;
                _disposed = true;
            }
//
//            DisposeAsync(new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token)
//                .GetAwaiter()
//                .GetResult();
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

                    _cmd = null;
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

        private async Task ExecuteOnlyAsync(IEnumerable<string> args, CancellationToken ct)
        {
            using (await _executeImpAsyncSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                await AddToExifToolAsync(null, args).ConfigureAwait(false);
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
                await _cmd.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(key))
                await _cmd.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
        }
    }
}