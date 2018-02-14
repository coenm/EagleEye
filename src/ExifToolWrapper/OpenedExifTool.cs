namespace EagleEye.ExifToolWrapper
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

        public OpenedExifTool(string exifToolPath)
        {
            _key = 0;
            _exifToolPath = exifToolPath;
            _defaultArgs = new List<string>
            {
                "-stay_open",
                "True",
                "-@",
                "-",
                ExifToolArguments.JSON_OUTPUT,
                ExifToolArguments.IGNORE_MINOR_ERRORS_AND_WARNINGS,
                ExifToolArguments.QUIET,
                ExifToolArguments.QUIET
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
                // cancel pending..
                // set state

                _cmd.StandardInput.WriteLine("-stay_open");
                _cmd.StandardInput.WriteLine("False");
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
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

        public async Task<string> Execute(string filename, IEnumerable<string> args)
        {
            var retries = 0;
            var tcs = new TaskCompletionSource<string>();

            while (retries < 10)
            {
                var key = Interlocked.Increment(ref _key).ToString();
                if (_waitingTasks.TryAdd(key, tcs))
                {
                    await AddToExifTool(key, args, filename).ConfigureAwait(false);
                    return await tcs.Task;
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

        private async Task AddToExifTool(string key, IEnumerable<string> args, string filename)
        {
            using (await _syncLockAddToExifTool.LockAsync().ConfigureAwait(false))
            {
                // todo check if ExifTool is open
                // etc etc

                foreach (var arg in args)
                    await _cmd.StandardInput.WriteLineAsync(arg).ConfigureAwait(false);

                await _cmd.StandardInput.WriteLineAsync(filename).ConfigureAwait(false);
                await _cmd.StandardInput.WriteLineAsync($"-execute{key}").ConfigureAwait(false);
            }
        }
    }
}