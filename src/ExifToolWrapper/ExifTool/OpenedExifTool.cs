namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifToolSimplified;

    using JetBrains.Annotations;

    using Nito.AsyncEx;

    public class OpenedExifTool : IExifTool
    {
        private readonly string _exifToolPath;
        private readonly AsyncLock _executeAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _executeImpAsyncSyncLock = new AsyncLock();
        private readonly AsyncLock _disposingSyncLock = new AsyncLock();
        private readonly object _cmdExitedSubscribedSyncLock = new object();
        private readonly object _initializedSyncLock = new object();
        private readonly CancellationTokenSource _stopQueueCts;

        private readonly List<string> _defaultArgs;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _waitingTasks;
        private readonly ExifToolStayOpenStream _stream;
        private IMedallionShell _cmd;
        private int _key;
        private bool _disposed;
        private bool _disposing;
        private bool _cmdExited;
        private bool _cmdExitedSubscribed;
        private bool _initialized;

        public OpenedExifTool(string exifToolPath)
        {
            _stream = new ExifToolStayOpenStream(Encoding.UTF8);

            _stopQueueCts = new CancellationTokenSource();
            _initialized = false;
            _disposed = false;
            _disposing = false;
            _cmdExited = false;
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

            lock (_initializedSyncLock)
            {
                if (_initialized)
                    return;

                _stream.Update += StreamOnUpdate;

                _cmd = CreateExitToolMedallionShell(_exifToolPath, _defaultArgs, _stream, null);

                _cmd.ProcessExited += CmdOnProcessExited;
                _cmdExitedSubscribed = true;
                _initialized = true;
            }
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default(CancellationToken))
        {
            if (!_initialized)
                throw new Exception("Not initialized");
            if (_disposed)
                throw new Exception("Disposed");
            if (_disposing)
                throw new Exception("Disposing");

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _stopQueueCts.Token);

            using (await _executeAsyncSyncLock.LockAsync(linkedCts.Token).ConfigureAwait(false))
            {
                return await ExecuteImpAsync(args, ct).ConfigureAwait(false);
            }
        }

        public async Task DisposeAsync(CancellationToken ct = default(CancellationToken))
        {
            if (!_initialized)
                return;

            if (_disposed)
                return;

            using (await _disposingSyncLock.LockAsync(ct).ConfigureAwait(false))
            {
                if (_disposed)
                    return;

                _disposing = true;
                Ignore(() => _stopQueueCts?.Cancel());

                try
                {
                    if (!_cmdExited)
                    {
                        // This is really not okay. Not sure why or when the stay-open False command doesn't seem to work.
                        // This is just a stupid 'workaround' and is okay for now.
                        await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);

                        var command = new[] { ExifToolArguments.STAY_OPEN, ExifToolArguments.BOOL_FALSE };
                        await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);

                        if (!_cmdExited)
                            await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);

                        var retry = 0;
                        while (retry < 3 && _cmdExited == false)
                        {
                            try
                            {
                                await ExecuteOnlyAsync(command, ct).ConfigureAwait(false);
                                if (!_cmdExited)
                                    await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
                            }
                            catch (Exception)
                            {
                                // ignore
                            }
                            finally
                            {
                                retry++;
                            }
                        }
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
                if (_cmdExited == false && _cmd?.Task != null)
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        _cmd.Kill();

                        // why?
                        await _cmd.Task.ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        sw.Stop();
                        Console.WriteLine($"Exception occurred after {sw.Elapsed} when awaiting ExifTool task. Msg: {e.Message}");
                        Ignore(() => _cmd.Kill());
                    }
                }

                _stream.Update -= StreamOnUpdate;
                Ignore(UnsubscribeCmdOnProcessExitedOnce);
                Ignore(() => _stream.Dispose());
                _cmd = null;
                _disposed = true;
                _disposing = false;
            }
        }

        protected virtual IMedallionShell CreateExitToolMedallionShell(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
        {
            return new MedallionShellAdapter(exifToolPath, defaultArgs, outputStream, errorStream);
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
                    _key++;
                    var key = _key.ToString();

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

        private async Task AddToExifToolAsync(string key, [NotNull] IEnumerable<string> args)
        {
            foreach (var arg in args)
                await _cmd.WriteLineAsync(arg).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(key))
                await _cmd.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
        }

        private void StreamOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            if (_waitingTasks.TryRemove(dataCapturedArgs.Key, out var tcs))
            {
                tcs.TrySetResult(dataCapturedArgs.Data);
            }
        }

        private void CmdOnProcessExited(object sender, EventArgs eventArgs)
        {
            _cmdExited = true;
            UnsubscribeCmdOnProcessExitedOnce();
        }

        private void UnsubscribeCmdOnProcessExitedOnce()
        {
            if (!_cmdExitedSubscribed)
                return;

            lock (_cmdExitedSubscribedSyncLock)
            {
                if (!_cmdExitedSubscribed)
                    return;
                Ignore(() => _cmd.ProcessExited -= CmdOnProcessExited);
                _cmdExitedSubscribed = false;
            }
        }
    }
}