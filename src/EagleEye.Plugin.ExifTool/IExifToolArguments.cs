namespace EagleEye.ExifTool
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    internal interface IExifToolArguments
    {
        [NotNull]
        IEnumerable<string> Arguments { get; }
    }
}
