namespace EagleEye.ExifToolWrapper.ExifToolSimplified
{
    using System.Threading.Tasks;

    using EagleEye.ExifToolWrapper.ExifTool;

    using Medallion.Shell;

    public class ClosedExifToolSimple
    {
        private readonly string _exifToolPath;

        public ClosedExifToolSimple(string exifToolPath)
        {
            _exifToolPath = exifToolPath;
        }

        public async Task<string> ExecuteAsync(object[] arguments)
        {
            var cmd = Command.Run(_exifToolPath, arguments);
            await cmd.Task.ConfigureAwait(false);

            if (cmd.Result.Success)
                return cmd.Result.StandardOutput;

            throw new ExiftoolException(cmd.Result.ExitCode, cmd.Result.StandardOutput, cmd.Result.StandardError);
        }
    }
}