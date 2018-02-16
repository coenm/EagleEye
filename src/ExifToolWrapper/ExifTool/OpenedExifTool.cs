namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Medallion.Shell;

    using Nito.AsyncEx;

    public class OpenedExifTool : IDisposable
    {
        private readonly string _exifToolPath;
        private readonly object _syncLock = new object();
        private readonly AsyncLock _syncLockAddToExifTool = new AsyncLock();
        private readonly List<string> _defaultArgs;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _waitingTasks;
        private ExifToolStayOpenStream _stream;
        private Command _cmd;
        private int _key;
        private bool _disposed;

        public OpenedExifTool(string exifToolPath)
        {
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
            lock (_syncLock)
            {
                _stream = new ExifToolStayOpenStream(Encoding.UTF8);
                _stream.Update += StreamOnUpdate;

                _cmd = Command.Run(_exifToolPath, _defaultArgs)
                    .RedirectTo(_stream);
            }
        }

        public void CancelPendingAndStop()
        {
            lock (_syncLock)
            {
                if (_disposed)
                    return;

                // cancel pending..
                // set state

                _cmd.StandardInput.WriteLine(ExifToolArguments.STAY_OPEN);
                _cmd.StandardInput.WriteLine(ExifToolArguments.BOOL_FALSE);
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                if (_disposed)
                    return;

                _disposed = true;

                _stream.Update -= StreamOnUpdate;

                if (!_cmd.Task.Wait(1000))
                    _cmd.Kill();

                _stream.Dispose();
                _stream = null;
                _cmd = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public async Task<string> ExecuteAsync(string filename, IEnumerable<string> args)
        {
            if (_disposed)
                throw new ObjectDisposedException($"{nameof(OpenedExifTool)} is already disposed or is disposing.");

            var retries = 0;
            var tcs = new TaskCompletionSource<string>();

            while (retries < 10)
            {
                var key = Interlocked.Increment(ref _key).ToString();
                if (_waitingTasks.TryAdd(key, tcs))
                {
                    var argsWithFilename = new List<string>(args) { filename };
                    await AddToExifToolAsync(key, argsWithFilename).ConfigureAwait(false);
                    return await tcs.Task.ConfigureAwait(false);
                }

                retries++;
            }

            throw new Exception("Could not execute");
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
            using (await _syncLockAddToExifTool.LockAsync().ConfigureAwait(false))
            {
                // todo check if ExifTool is open
                // etc etc

                foreach (var arg in args)
                    await _cmd.StandardInput.WriteLineAsync(arg).ConfigureAwait(false);

                await _cmd.StandardInput.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
            }
        }
    }
}