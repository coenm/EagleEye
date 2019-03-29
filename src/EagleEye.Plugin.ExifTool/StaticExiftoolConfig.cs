namespace EagleEye.ExifTool
{
    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class StaticExiftoolConfig : IExifToolConfig
    {
        public StaticExiftoolConfig([NotNull] string exifToolExe)
        {
            Dawn.Guard.Argument(exifToolExe, nameof(exifToolExe)).NotNull().NotWhiteSpace();
            ExifToolExe = exifToolExe;
        }

        public string ExifToolExe { get; }
    }
}
