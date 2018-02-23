namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Medallion.Shell;

    public class MedallionShellAdapter : IMedallionShell
    {
        private readonly Command _cmd;
        private readonly CancellationTokenSource _cts;

        public MedallionShellAdapter(string exifToolPath, List<string> defaultArgs, Stream outputStream, Stream errorStream)
        {
            _cts = new CancellationTokenSource();
            _cmd = Command.Run(exifToolPath, defaultArgs.ToArray(), options: o => o.CancellationToken(_cts.Token).ThrowOnError())
                          .RedirectTo(outputStream)
                          /*.RedirectStandardErrorTo(errorStream)*/;
        }

        public Task<CommandResult> Task => _cmd.Task;

        public Command Command => _cmd;

        public void KillAfter(TimeSpan delay)
        {
            _cts.CancelAfter(delay);
        }

        public void Kill()
        {
            _cmd.Kill();
        }

        public Task WriteLineAsync(string text)
        {
            return _cmd.StandardInput.WriteLineAsync(text);
        }

        public void WriteLine(string s)
        {
            _cmd.StandardInput.WriteLine(s);
        }
    }
}