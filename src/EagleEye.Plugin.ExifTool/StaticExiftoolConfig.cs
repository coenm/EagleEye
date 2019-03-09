namespace EagleEye.ExifTool
{
    using Helpers.Guards;
    using JetBrains.Annotations;

    internal class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe)
        {
            Guard.NotNullOrWhiteSpace(exifToolExe, nameof(exifToolExe));
            ExifToolExe = exifToolExe;
        }

        public string ExifToolExe { get; }
    }
}
