namespace EagleEye.ExifTool
{
    using JetBrains.Annotations;

    internal interface IExifToolConfig
    {
        string ExifToolExe { get; }

        [CanBeNull]
        string ExifToolConfigFile { get; }
    }
}
