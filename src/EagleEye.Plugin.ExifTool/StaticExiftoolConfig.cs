namespace EagleEye.ExifTool
{
    using Dawn;
    using JetBrains.Annotations;

    internal class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe, [CanBeNull] string configFile)
        {
            Guard.Argument(exifToolExe, nameof(exifToolExe)).NotNull().NotWhiteSpace();
            ExifToolExe = exifToolExe;
            ExifToolConfigFile = configFile;
        }

        public string ExifToolExe { get; }

        [CanBeNull]
        public string ExifToolConfigFile { get; }
    }
}
