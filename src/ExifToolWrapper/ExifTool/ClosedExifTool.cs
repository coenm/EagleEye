namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Medallion.Shell;

    public class ClosedExifTool : IExifTool
    {
        private readonly string _exifToolPath;
        private bool _disposed;

        public ClosedExifTool(string exifToolPath)
        {
            _exifToolPath = exifToolPath;
            _disposed = false;
        }

        public void Init()
        {
            // Nothing needed for initialization.
        }

        public async Task<string> ExecuteAsync(IEnumerable<string> args, CancellationToken ct = default(CancellationToken))
        {
            if (_disposed)
                throw new ObjectDisposedException("Disposed");

            var cmd = Command.Run(_exifToolPath, args);
            await cmd.Task.ConfigureAwait(false);

            if (cmd.Result.Success)
                return cmd.Result.StandardOutput;

            throw new ExiftoolException(cmd.Result.ExitCode, cmd.Result.StandardOutput, cmd.Result.StandardError);
        }

        public Task DisposeAsync(CancellationToken ct = default(CancellationToken))
        {
            _disposed = true;
            return Task.CompletedTask;
        }
    }
}