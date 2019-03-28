namespace EagleEye.ExifTool
{
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(exifToolExe, nameof(exifToolExe));
            ExifToolExe = exifToolExe;
        }

        public string ExifToolExe { get; }
    }
}
