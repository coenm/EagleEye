namespace EagleEye.ExifToolWrapper.ExifTool
{
    using System;

    using Medallion.Shell;

    public class ClosedExifToolSimple
    {
        private readonly string _exifToolPath;

        public ClosedExifToolSimple(string exifToolPath)
        {
            _exifToolPath = exifToolPath;
        }

        public string Execute(object[] arguments)
        {
            var cmd = Command.Run(_exifToolPath, arguments);

            if (cmd.Task.Wait(TimeSpan.FromSeconds(20)))
                return cmd.Result.StandardOutput;

            cmd.Kill();
            throw new Exception("Could not close Exiftool without killing it.");
        }
    }
}