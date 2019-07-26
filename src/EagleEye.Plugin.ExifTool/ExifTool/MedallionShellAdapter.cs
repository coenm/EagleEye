namespace EagleEye.ExifTool.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Dawn;
    using JetBrains.Annotations;
    using Medallion.Shell;

    public class MedallionShellAdapter : IMedallionShell
    {
        [NotNull]
        private readonly Command cmd;

        public MedallionShellAdapter(
            string executable,
            IEnumerable<string> defaultArgs,
            [NotNull] Stream outputStream,
            [CanBeNull] Stream errorStream = null)
        {
            Guard.Argument(executable, nameof(executable)).NotNull().NotEmpty();
            Guard.Argument(outputStream, nameof(outputStream)).NotNull();

            if (errorStream == null)
            {
                cmd = Command.Run(executable, defaultArgs)
                             .RedirectTo(outputStream);
            }
            else
            {
                cmd = Command.Run(executable, defaultArgs)
                             .RedirectTo(outputStream)
                             .RedirectStandardErrorTo(errorStream);
            }

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

        [CanBeNull]
        public event EventHandler ProcessExited;

        public bool Finished => Task.IsCompleted;

        [NotNull]
        public Task<CommandResult> Task { get; }

        public void Kill()
        {
            cmd.Kill();
        }

        public Task WriteLineAsync([NotNull] string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return cmd.StandardInput.WriteLineAsync(text);
        }
    }
}
