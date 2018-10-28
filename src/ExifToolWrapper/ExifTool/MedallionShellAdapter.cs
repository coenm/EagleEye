namespace EagleEye.ExifToolWrapper.ExifTool
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
        private readonly Command cmd;

        public MedallionShellAdapter(string exifToolPath, IEnumerable<string> defaultArgs, Stream outputStream, Stream errorStream = null)
        {
            cmd = Command.Run(exifToolPath, defaultArgs)
                          .RedirectTo(outputStream);

            if (errorStream != null)
                cmd = cmd.RedirectStandardErrorTo(errorStream);

            Task = System.Threading.Tasks.Task.Run(async () =>
                                                   {
                                                       try
                                                       {
                                                           return await cmd.Task.ConfigureAwait(false);
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
            cmd.Kill();
        }

        public Task WriteLineAsync(string text)
        {
            return cmd.StandardInput.WriteLineAsync(text);
        }
    }
}
