namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Medallion.Shell;

    public class MedallionShellAdapter : IMedallionShell
    {
        [NotNull]
        private readonly Command _cmd;

        public MedallionShellAdapter(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream = null)
        {
            _cmd = Command.Run(exifToolPath, defaultArgs)
                          .RedirectTo(outputStream);

            if (errorStream != null)
                _cmd = _cmd.RedirectStandardErrorTo(errorStream);


            Task = System.Threading.Tasks.Task.Run(async () =>
                                                   {
                                                       try
                                                       {
                                                           return await _cmd.Task.ConfigureAwait(false);
                                                       }
                                                       finally
                                                       {
                                                           ProcessExited?.Invoke(this, EventArgs.Empty);
                                                       }
                                                   });
        }



        public event EventHandler ProcessExited = delegate { };

        public bool Finished => Task.IsCompleted;

        [NotNull]
        public Task<CommandResult> Task { get; }

        public void Kill()
        {
            _cmd.Kill();
        }

        public Task WriteLineAsync(string text)
        {
            return _cmd.StandardInput.WriteLineAsync(text);
        }
    }
}